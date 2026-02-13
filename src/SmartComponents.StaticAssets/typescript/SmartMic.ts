import { extractFormConfig, populateForm, sendToInferenceAPI } from './SmartFormUtil';

/**
 * States for the SmartMic dialog
 */
enum SmartMicState {
    Idle = 'idle',
    Listening = 'listening',
    Review = 'review',
    Processing = 'processing',
    Error = 'error'
}

/**
 * Web Speech API type declarations
 */
declare global {
    interface Window {
        SpeechRecognition: any;
        webkitSpeechRecognition: any;
        mozSpeechRecognition: any;
        msSpeechRecognition: any;
    }
}

/**
 * Registers click handler for all smart-mic-button triggers
 */
export function registerSmartMicClickHandler() {
    document.addEventListener('click', (evt) => {
        const target = evt.target;
        if (target instanceof Element) {
            const button = target.closest('button[data-smart-mic-trigger=true]');
            if (button instanceof HTMLButtonElement) {
                openSmartMicDialog(button);
            }
        }
    });
}

/**
 * Opens the Smart Mic dialog and initiates the voice input flow
 */
async function openSmartMicDialog(button: HTMLButtonElement) {
    const form = button.closest('form');
    if (!form) {
        console.error('A smart mic button was clicked, but it is not inside a form');
        return;
    }

    const formConfig = extractFormConfig(form);
    if (formConfig.length === 0) {
        console.warn('A smart mic button was clicked, but no fields were found in its form');
        return;
    }

    // Check if Speech Recognition is available
    if (!isSpeechRecognitionAvailable()) {
        alert('Speech recognition is not supported in your browser. Please use Chrome, Edge, or Safari.');
        return;
    }

    // Create and show dialog
    const dialog = createSmartMicDialog(button, form, formConfig);
    document.body.appendChild(dialog);

    // Focus trap and ESC handling
    setupDialogAccessibility(dialog);

    // Show dialog
    dialog.showModal();
    startListening(dialog, button, form, formConfig);
}


/**
 * Creates the Smart Mic dialog element
 */
function createSmartMicDialog(_button: HTMLButtonElement, _form: HTMLFormElement, _formConfig: any[]): HTMLDialogElement {
    const dialog = document.createElement('dialog');
    dialog.className = 'smart-mic-dialog';
    dialog.setAttribute('aria-labelledby', 'smart-mic-title');
    dialog.setAttribute('aria-describedby', 'smart-mic-description');

    dialog.innerHTML = `
        <div class="smart-mic-content">
            <div class="smart-mic-header">
                <h2 id="smart-mic-title">Voice Input</h2>
                <button type="button" class="smart-mic-close" aria-label="Close dialog">
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M18 6L6 18M6 6l12 12"/>
                    </svg>
                </button>
            </div>
            
            <div id="smart-mic-description" class="smart-mic-body">
                <div class="smart-mic-state-listening" data-state="listening">
                    <div class="smart-mic-visual-indicator">
                        <div class="smart-mic-pulse-ring"></div>
                        <div class="smart-mic-pulse-ring"></div>
                        <div class="smart-mic-pulse-ring"></div>
                        <svg class="smart-mic-icon-large" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" fill="currentColor">
                            <path d="M3.5 6.5A.5.5 0 0 1 4 7v1a4 4 0 0 0 8 0V7a.5.5 0 0 1 1 0v1a5 5 0 0 1-4.5 4.975V15h3a.5.5 0 0 1 0 1h-7a.5.5 0 0 1 0-1h3v-2.025A5 5 0 0 1 3 8V7a.5.5 0 0 1 .5-.5z"/>
                            <path d="M10 8a2 2 0 1 1-4 0V3a2 2 0 1 1 4 0v5zM8 0a3 3 0 0 0-3 3v5a3 3 0 0 0 6 0V3a3 3 0 0 0-3-3z"/>
                        </svg>
                    </div>
                    <p class="smart-mic-instruction">Listening... Speak now</p>
                    <p class="smart-mic-live-transcript"></p>
                </div>
                
                <div class="smart-mic-state-review" data-state="review" style="display: none;">
                    <label for="smart-mic-transcript">
                        Review and edit your transcription:
                    </label>
                    <textarea 
                        id="smart-mic-transcript" 
                        class="smart-mic-transcript-edit"
                        rows="4"
                        placeholder="Your transcription will appear here..."
                    ></textarea>
                </div>
                
                <div class="smart-mic-state-processing" data-state="processing" style="display: none;">
                    <div class="smart-mic-spinner"></div>
                    <p>Processing your input...</p>
                </div>
                
                <div class="smart-mic-state-error" data-state="error" style="display: none;">
                    <svg class="smart-mic-error-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <circle cx="12" cy="12" r="10"/>
                        <line x1="12" y1="8" x2="12" y2="12"/>
                        <line x1="12" y1="16" x2="12.01" y2="16"/>
                    </svg>
                    <p class="smart-mic-error-message"></p>
                </div>
            </div>
            
            <div class="smart-mic-footer">
                <button type="button" class="smart-mic-button smart-mic-button-secondary smart-mic-cancel">
                    Cancel
                </button>
                <button type="button" class="smart-mic-button smart-mic-button-primary smart-mic-stop" style="display: none;">
                    Stop Listening
                </button>
                <button type="button" class="smart-mic-button smart-mic-button-primary smart-mic-apply" style="display: none;">
                    Apply to Form
                </button>
            </div>
        </div>
    `;

    return dialog;
}

/**
 * Sets up accessibility features for the dialog
 */
function setupDialogAccessibility(dialog: HTMLDialogElement): void {
    // ESC key handling
    dialog.addEventListener('keydown', (event) => {
        if (event.key === 'Escape') {
            closeDialog(dialog);
        }
    });

    // Close button
    const closeButton = dialog.querySelector('.smart-mic-close');
    closeButton?.addEventListener('click', () => closeDialog(dialog));

    // Cancel button
    const cancelButton = dialog.querySelector('.smart-mic-cancel');
    cancelButton?.addEventListener('click', () => closeDialog(dialog));
}

/**
 * Checks if Speech Recognition API is available
 */
/**
 * Gets the browser-specific Speech Recognition constructor
 */
function getSpeechRecognitionConstructor(): any {
    return (window as any).SpeechRecognition ||
        (window as any).webkitSpeechRecognition ||
        (window as any).mozSpeechRecognition ||
        (window as any).msSpeechRecognition;
}

/**
 * Checks if Speech Recognition API is available
 */
function isSpeechRecognitionAvailable(): boolean {
    const SpeechRecognition = getSpeechRecognitionConstructor();
    return !!SpeechRecognition && typeof SpeechRecognition === 'function';
}

/**
 * Starts listening to user speech
 */
function startListening(dialog: HTMLDialogElement, button: HTMLButtonElement, form: HTMLFormElement, formConfig: any[]): void {
    const SpeechRecognition = getSpeechRecognitionConstructor();
    if (!SpeechRecognition) {
        showError(dialog, 'Speech recognition is not supported in this browser. Please use Chrome, Edge, or Safari.');
        return;
    }
    const recognition = new SpeechRecognition();

    // Configuration
    recognition.continuous = true;
    recognition.interimResults = true;
    recognition.lang = 'en-US'; // TODO: Make configurable
    recognition.maxAlternatives = 1;

    let finalTranscript = '';
    let interimTranscript = '';
    let hasDetectedSpeech = false;

    recognition.onstart = () => {
        setState(dialog, SmartMicState.Listening);
        console.log('[SmartMic] Recognition started - speak now!');
    };

    recognition.onresult = (event: any) => {
        interimTranscript = '';
        hasDetectedSpeech = true; // We got some results

        for (let i = event.resultIndex; i < event.results.length; i++) {
            const transcript = event.results[i][0].transcript;
            if (event.results[i].isFinal) {
                finalTranscript += transcript + ' ';
                console.log('[SmartMic] Final transcript:', transcript);
            } else {
                interimTranscript += transcript;
            }
        }

        // Update live transcript display
        const liveTranscriptElement = dialog.querySelector('.smart-mic-live-transcript');
        if (liveTranscriptElement) {
            const fullText = (finalTranscript + interimTranscript).trim();
            liveTranscriptElement.textContent = fullText || 'Listening...';
        }
    };

    recognition.onerror = (event: any) => {
        console.error('[SmartMic] Speech recognition error:', event.error);
        let errorMessage = 'An error occurred during speech recognition.';

        if (event.error === 'not-allowed' || event.error === 'permission-denied') {
            errorMessage = 'Microphone access was denied. Please enable microphone access and try again.';
        } else if (event.error === 'no-speech') {
            // Don't treat this as fatal - just log it
            console.warn('[SmartMic] No speech detected by browser, but continuing...');
            return; // Don't show error yet
        } else if (event.error === 'network') {
            errorMessage = 'Network error occurred. Please check your connection.';
        } else if (event.error === 'aborted') {
            // User manually stopped, not an error
            return;
        }

        showError(dialog, errorMessage);
    };

    recognition.onend = () => {
        console.log('[SmartMic] Recognition ended');
        // When recognition ends, show review state with final transcript
        const fullTranscript = finalTranscript.trim();
        if (fullTranscript) {
            showReviewState(dialog, fullTranscript, button, form, formConfig);
        } else if (hasDetectedSpeech) {
            // We detected some speech but it wasn't finalized
            const anyText = (finalTranscript + interimTranscript).trim();
            if (anyText) {
                showReviewState(dialog, anyText, button, form, formConfig);
            } else {
                showError(dialog, 'No clear speech was captured. Please speak more clearly and try again.');
            }
        } else {
            showError(dialog, 'No speech was detected. Please check your microphone and try again.');
        }
    };

    // Stop button
    const stopButton = dialog.querySelector('.smart-mic-stop');
    if (stopButton) {
        stopButton.addEventListener('click', () => {
            console.log('[SmartMic] User stopped recording');
            recognition.stop();
        }, { once: true });
    }

    try {
        recognition.start();
        console.log('[SmartMic] Starting speech recognition...');
    } catch (error) {
        console.error('[SmartMic] Failed to start recognition:', error);
        showError(dialog, 'Failed to start microphone. Please try again.');
    }
}

/**
 * Shows the review state where user can edit the transcript
 */
function showReviewState(dialog: HTMLDialogElement, transcript: string, button: HTMLButtonElement, form: HTMLFormElement, formConfig: any[]): void {
    setState(dialog, SmartMicState.Review);

    const textareaElement = dialog.querySelector('.smart-mic-transcript-edit') as HTMLTextAreaElement;
    if (textareaElement) {
        textareaElement.value = transcript;
        textareaElement.focus();
    }

    // Apply button
    const applyButton = dialog.querySelector('.smart-mic-apply');
    if (applyButton) {
        applyButton.addEventListener('click', async () => {
            const editedTranscript = textareaElement?.value.trim();
            if (editedTranscript) {
                await applyTranscriptToForm(dialog, editedTranscript, button, form, formConfig);
            }
        }, { once: true });
    }
}

/**
 * Applies the transcript to the form via the inference API
 */
async function applyTranscriptToForm(dialog: HTMLDialogElement, transcript: string, button: HTMLButtonElement, form: HTMLFormElement, formConfig: any[]): Promise<void> {
    setState(dialog, SmartMicState.Processing);

    try {
        button.disabled = true;
        const url = button.getAttribute('data-url');
        if (!url) {
            throw new Error('SmartMic button is missing data-url attribute');
        }

        const antiforgeryName = button.getAttribute('data-antiforgery-name');
        const antiforgeryValue = button.getAttribute('data-antiforgery-value');

        const response = await sendToInferenceAPI(url, formConfig, transcript, antiforgeryName, antiforgeryValue);
        const responseText = await response.text();

        // Populate the form
        populateForm(form, formConfig, responseText);

        // Close dialog on success
        closeDialog(dialog);
    } catch (error) {
        console.error('Error applying transcript to form:', error);
        showError(dialog, 'Failed to process your input. Please try again.');
    } finally {
        button.disabled = false;
    }
}

/**
 * Sets the dialog state and updates UI accordingly
 */
function setState(dialog: HTMLDialogElement, state: SmartMicState): void {
    // Hide all state elements
    dialog.querySelectorAll('[data-state]').forEach(el => {
        if (el instanceof HTMLElement) {
            el.style.display = 'none';
        }
    });

    // Show the appropriate state element
    const stateElement = dialog.querySelector(`[data-state="${state}"]`);
    if (stateElement instanceof HTMLElement) {
        stateElement.style.display = 'block';
    }

    // Update button visibility
    const stopButton = dialog.querySelector('.smart-mic-stop');
    const applyButton = dialog.querySelector('.smart-mic-apply');

    if (stopButton instanceof HTMLElement) {
        stopButton.style.display = state === SmartMicState.Listening ? 'inline-block' : 'none';
    }

    if (applyButton instanceof HTMLElement) {
        applyButton.style.display = state === SmartMicState.Review ? 'inline-block' : 'none';
    }
}

/**
 * Shows an error state in the dialog
 */
function showError(dialog: HTMLDialogElement, message: string): void {
    setState(dialog, SmartMicState.Error);

    const errorMessageElement = dialog.querySelector('.smart-mic-error-message');
    if (errorMessageElement) {
        errorMessageElement.textContent = message;
    }
}

/**
 * Closes the dialog and cleans up
 */
function closeDialog(dialog: HTMLDialogElement): void {
    dialog.close();

    // Remove from DOM after animation
    setTimeout(() => {
        dialog.remove();
    }, 300);
}
