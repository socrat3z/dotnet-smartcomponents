import { extractFormConfig, populateForm, sendToInferenceAPI } from './SmartFormUtil';

export function registerSmartPasteClickHandler() {
    document.addEventListener('click', (evt) => {
        const target = evt.target;
        if (target instanceof Element) {
            const button = target.closest('button[data-smart-paste-trigger=true]');
            if (button instanceof HTMLButtonElement) {
                performSmartPaste(button);
            }
        }
    });
}

async function performSmartPaste(button: HTMLButtonElement) {
    const form = button.closest('form');
    if (!form) {
        console.error('A smart paste button was clicked, but it is not inside a form');
        return;
    }

    const formConfig = extractFormConfig(form);
    if (formConfig.length == 0) {
        console.warn('A smart paste button was clicked, but no fields were found in its form');
        return;
    }

    const clipboardContents = await readClipboardText();
    if (!clipboardContents) {
        console.info('A smart paste button was clicked, but no data was found on the clipboard');
        return;
    }

    try {
        button.disabled = true;
        const url = button.getAttribute('data-url');
        if (!url) {
            throw new Error('SmartPaste button is missing data-url attribute');
        }

        const antiforgeryName = button.getAttribute('data-antiforgery-name');
        const antiforgeryValue = button.getAttribute('data-antiforgery-value');

        const response = await sendToInferenceAPI(url, formConfig, clipboardContents, antiforgeryName, antiforgeryValue);
        const responseText = await response.text();
        populateForm(form, formConfig, responseText);
    } finally {
        button.disabled = false;
    }
}

async function readClipboardText(): Promise<string | null> {
    const fake = document.getElementById('fake-clipboard') as HTMLInputElement;
    if (fake?.value) {
        return fake.value;
    }

    if (!navigator.clipboard.readText) {
        alert('The current browser does not support reading the clipboard.\n\nTODO: Implement alternate UI for this case.');
        return null;
    }

    return navigator.clipboard.readText();
}
