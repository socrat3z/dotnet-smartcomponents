import { isComboBox, setFormElementValueWithEvents } from './FormUtil';

/**
 * Configuration describing a form field for smart component inference
 */
export interface FieldConfig {
    identifier: string;
    description: string | null;
    element: HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement;
    type: 'string' | 'boolean' | 'number' | 'fixed-choices';
    allowedValues?: string[];
    [key: string]: unknown;
}

/**
 * Extracts form field configuration from a form element
 */
export function extractFormConfig(form: HTMLFormElement): FieldConfig[] {
    const fields: FieldConfig[] = [];
    let unidentifiedCount = 0;
    form.querySelectorAll('input, select, textarea').forEach(element => {
        if (!(element instanceof HTMLInputElement || element instanceof HTMLSelectElement || element instanceof HTMLTextAreaElement)) {
            return;
        }

        if (element.type === 'hidden' || isComboBox(element)) {
            return;
        }

        const isRadio = element.type === 'radio';
        const identifier = isRadio
            ? element.name
            : element.id || element.name || `unidentified_${++unidentifiedCount}`;

        // Only include one field for each related set of radio buttons
        if (isRadio && fields.find(f => f.identifier === identifier)) {
            return;
        }

        let description: string | null = null;
        if (!isRadio) {
            description = inferFieldDescription(form, element);
            if (!description) {
                // If we can't say anything about what this field represents, we have to exclude it
                return;
            }
        }

        const fieldEntry: FieldConfig = {
            identifier: identifier,
            description: description,
            element: element,
            type: element.type === 'checkbox' ? 'boolean'
                : element.type === 'number' ? 'number' : 'string',
        };

        if (element instanceof HTMLSelectElement) {
            const options = Array.prototype.filter.call(element.querySelectorAll('option'), (o: HTMLOptionElement) => !!o.value) as HTMLOptionElement[];
            fieldEntry.allowedValues = Array.prototype.map.call(options, (o: HTMLOptionElement) => o.textContent ?? '') as string[];
            fieldEntry.type = 'fixed-choices';
        } else if (isRadio) {
            fieldEntry.allowedValues = [];
            fieldEntry.type = 'fixed-choices';
            Array.prototype.forEach.call(form.querySelectorAll('input[type=radio]'), (e: HTMLInputElement) => {
                if (e.name === identifier) {
                    const choiceDescription = inferFieldDescription(form, e);
                    if (choiceDescription && fieldEntry.allowedValues) {
                        fieldEntry.allowedValues.push(choiceDescription);
                    }
                }
            });
        }

        fields.push(fieldEntry);
    });

    return fields;
}

/**
 * Populates form fields with values from inference response
 */
export function populateForm(form: HTMLFormElement, formConfig: FieldConfig[], responseText: string): void {
    let resultData: any;
    try {
        resultData = JSON.parse(responseText);
    } catch {
        return;
    }

    formConfig.forEach(field => {
        // For missing fields, it's usually better to leave the existing field data in place, since there
        // might be useful values in unrelated fields. It would be nice if the inference could conclusively
        // determine cases when a field should be cleared, but in most cases it can't distinguish "no
        // information available" from "the value should definitely be blanked out".
        let value = resultData[field.identifier];
        if (value !== undefined && value !== null) {
            value = value.toString().trim();
            if (field.element instanceof HTMLInputElement && field.element.type === 'radio') {
                // Radio is a bit more complex than the others as it's not just a single form element
                // We have to find the one corresponding to the new value, which in turn depends on
                // how we're interpreting the field description
                const radioInputToSelect = findInputRadioByText(form, field.element.name, value);
                if (radioInputToSelect) {
                    setFormElementValueWithEvents(radioInputToSelect, true);
                }
            } else {
                setFormElementValueWithEvents(field.element, value);
            }
        }
    });
}

/**
 * Sends text content to the smart paste API/endpoint for inference
 */
export async function sendToInferenceAPI(
    url: string,
    formConfig: FieldConfig[],
    textContent: string,
    antiforgeryName: string | null,
    antiforgeryValue: string | null
): Promise<Response> {
    const formFields = formConfig.map(entry => restrictProperties(entry as Record<string, unknown>, ['identifier', 'description', 'allowedValues', 'type']));

    const body: Record<string, string> = {
        dataJson: JSON.stringify({
            formFields,
            clipboardContents: textContent, // Uses same param name as SmartPaste for backend compatibility
        })
    };

    if (antiforgeryName && antiforgeryValue) {
        body[antiforgeryName] = antiforgeryValue;
    }

    return fetch(url, {
        method: 'post',
        headers: {
            'content-type': 'application/x-www-form-urlencoded',
        },
        body: new URLSearchParams(body)
    });
}

function inferFieldDescription(form: HTMLFormElement, element: HTMLElement): string | null {
    // If there's explicit config, use it
    const smartPasteDescription = element.getAttribute('data-smartpaste-description');
    if (smartPasteDescription) {
        return smartPasteDescription;
    }

    // If there's an explicit label, use it
    const labels = element.id && form.querySelectorAll(`label[for='${element.id}']`);
    if (labels && labels.length === 1) {
        const firstLabel = labels[0];
        if (firstLabel) {
            return firstLabel.textContent?.trim() ?? null;
        }
    }

    // Try searching up the DOM hierarchy to look for some container that only contains
    // this one field and has text
    let candidateContainer = element.parentElement;
    while (candidateContainer && candidateContainer !== form.parentElement) {
        const inputsInContainer = candidateContainer.querySelectorAll('input, select, textarea');
        if (inputsInContainer.length === 1 && inputsInContainer[0] === element) {
            // Here's a container in which this element is the only input. Any text here
            // will be assumed to describe the input.
            let text = candidateContainer.textContent.replace(/\s+/g, ' ').trim();
            if (text) {
                return text;
            }
        }

        candidateContainer = candidateContainer.parentElement;
    }

    // Fall back on name (because that's what would be bound on the server) or even ID
    // If even these have no data, we won't be able to use the field
    return element.getAttribute('name') || element.id;
}

function findInputRadioByText(form: HTMLFormElement, radioGroupName: string, valueText: string): HTMLInputElement | null {
    const candidates = Array.from(form.querySelectorAll('input[type=radio]'))
        .filter(e => e instanceof HTMLInputElement && e.name === radioGroupName)
        .map(e => ({ elem: e as HTMLInputElement, text: inferFieldDescription(form, e as HTMLInputElement) }));
    const exactMatches = candidates.filter(o => o.text === valueText);
    if (exactMatches.length > 0) {
        const firstMatch = exactMatches[0];
        if (firstMatch) {
            return firstMatch.elem;
        }
    }

    const partialMatches = candidates.filter(o => o.text && o.text.indexOf(valueText) >= 0);
    if (partialMatches.length === 1) {
        const firstPartialMatch = partialMatches[0];
        if (firstPartialMatch) {
            return firstPartialMatch.elem;
        }
    }

    return null;
}

function restrictProperties(object: Record<string, unknown>, propertyNames: string[]): Record<string, unknown> {
    const result: Record<string, unknown> = {};
    propertyNames.forEach(propertyName => {
        const value = object[propertyName];
        if (value !== undefined) {
            result[propertyName] = value;
        }
    });
    return result;
}
