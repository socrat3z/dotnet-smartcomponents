export class SmartTranslate {
    static async getText(targetId: string): Promise<string> {
        const element = document.getElementById(targetId) as HTMLElement;
        if (!element) return "";

        // If there's a selection, prioritize that
        // Note: selectionStart/End only exist on input/textarea
        if (element instanceof HTMLInputElement || element instanceof HTMLTextAreaElement) {
            const start = element.selectionStart;
            const end = element.selectionEnd;
            if (start !== null && end !== null && start !== end) {
                return element.value.substring(start, end);
            }
            return element.value;
        }

        return element.innerText || "";
    }

    static async replaceText(targetId: string, newText: string): Promise<void> {
        const element = document.getElementById(targetId) as HTMLElement;
        if (!element) return;

        if (element instanceof HTMLInputElement || element instanceof HTMLTextAreaElement) {
            const start = element.selectionStart;
            const end = element.selectionEnd;

            if (start !== null && end !== null && start !== end) {
                // Replace selection
                const currentVal = element.value;
                element.value = currentVal.substring(0, start) + newText + currentVal.substring(end);
                // Dispatch input event to notify frameworks (React, Blazor, etc)
                element.dispatchEvent(new Event('input', { bubbles: true }));
                element.dispatchEvent(new Event('change', { bubbles: true }));
            } else {
                // Replace all
                element.value = newText;
                element.dispatchEvent(new Event('input', { bubbles: true }));
                element.dispatchEvent(new Event('change', { bubbles: true }));
            }
        } else {
            element.innerText = newText;
        }
    }

    static showModal(dialog: HTMLDialogElement): void {
        if (dialog instanceof HTMLDialogElement && !dialog.open) {
            dialog.showModal();
        }
    }

    static closeModal(dialog: HTMLDialogElement): void {
        if (dialog instanceof HTMLDialogElement && dialog.open) {
            dialog.close();
        }
    }
}

// Global export for Blazor to find
(window as any).SmartTranslate = SmartTranslate;
