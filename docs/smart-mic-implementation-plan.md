# SmartMic: Feature Specification & Implementation Plan

## 1. Feature Overview

**SmartMic** is a voice-enabled input component that allows users to dictate information to fill out forms. It serves as an alternative input method to "Smart Paste," leveraging the same underlying "Smart Components" inference engine to map unstructured text (transcribed from speech) to structured form fields.

**Target User:** Mobile users, field agents, or anyone who finds typing long-form content on a screen cumbersome.
**Core Value:** Speed (3x faster than typing) and accessibility.

## 2. Architecture & Design Principles

### Core insight
The backend logic for `SmartMic` is identical to `SmartPaste`. Both features follow the pattern:
`Unstructured Text` -> `LLM Inference` -> `Structured Form Data`

The only difference is the *source* of the unstructured text:
*   **Smart Paste:** Clipboard.
*   **Smart Mic:** Speech-to-Text (STT) transcription.

Therefore, we will **reuse** the existing `SmartPaste` backend endpoints (`_smartcomponents/smartpaste`) and inference logic (`SmartPasteInference`). The majority of new work is on the frontend (Blazor/JS).

### Frontend Strategy
*   **Speech-to-Text:** We will use the browser's native **Web Speech API (`SpeechRecognition`)** for the MVP. This ensures zero latency and no additional server-side audio processing costs.
*   **UI/UX:** A "Button + Modal" pattern.
    *   **Button:** Triggers the flow.
    *   **Modal:** Handles the "Listen -> Preview -> Edit -> Confirm" loop. This is critical for building user trust in voice input.

## 3. Component Design

### 3.1. `SmartMicButton.razor` / `<smart-mic-button>`
A simple trigger component, analogous to `SmartPasteButton`.

*   **Responsibility:** Render the microphone icon and trigger the `SmartMicDialog`.
*   **Parameters:**
    *   `DefaultIcon` (bool): Whether to show the built-in mic icon.
    *   `ChildContent` (RenderFragment): Custom content/label.

### 3.2. `SmartMicDialog.razor`
A shared modal component that handles the interaction state.

*   **Visual States:**
    1.  **Listening:** Shows a visual indicator (pulsing ring or waveform) to verify the mic is active.
    2.  **Processing:** Shows a spinner while STT catches up (usually near-instant).
    3.  **Review (Edit):** Displays the transcribed text in a large `<textarea>`. Users *must* be able to manually edit errors here.
    4.  **Inference:** Locks the UI while the backend LLM processes the text.
*   **Actions:**
    *   `Start/Stop` recording.
    *   `Apply`: Submits the text to the form filling logic.
    *   `Cancel`: Closes the modal without action.

### 3.3. JavaScript Interop (`SmartComponents.AspNetCore.Components.lib.module.js`)
We need to refactor the existing monolithic `smart-paste` logic.

*   **Extraction:** Extract the "Form Scraping" and "Form Filling" logic into reusable functions.
    *   `extractFormFields(formElement)`: Returns the JSON structure of the form.
    *   `fillForm(formElement, fieldValues)`: Populates the DOM elements.
    *   **New:** `recognizeSpeech(onResult, onError)`: usage of `window.SpeechRecognition`.

## 4. Implementation Steps

### Phase 1: JS Refactoring (Prerequisite)
The current `SmartComponents.AspNetCore.Components.lib.module.js` contains a large async function attached to the smart paste button click. We must break this apart.

1.  **Extract `findTargetForm(button)`**: Locate the parent form.
2.  **Extract `scrapeFormFields(form)`**: The logic that iterates inputs/selects/textareas and builds the JSON descriptor.
3.  **Extract `smartFillForm(form, clipboardText)`**:
    *   This currently calls `fetch` to `_smartcomponents/smartpaste`.
    *   We need to change the signature to `smartFillForm(form, textContent)`.
    *   The CLIPBOARD reading logic remains in the `SmartPaste` event handler, which then calls `smartFillForm`.

### Phase 2: The SmartMic Component
1.  **Create `SmartMicButton.razor`**:
    *   Similar API to `SmartPasteButton`.
    *   Injects a scoped service or uses a cascading parameter to talk to the Dialog (or renders the Dialog itself if we go for a self-contained approach). *Decision: Render the dialog as part of the button's shadow DOM or use a portal approach. For simplicity in Blazor, `SmartMicButton` can render a standard accessible HTML `<dialog>` element.*

2.  **Implement `SmartMicDialog` UI**:
    *   Use standard HTML/CSS for the modal.
    *   **Accessibility:** Ensure focus trapping and ARIA roles.
    *   **Styling:** Matching the "Senior Product Owner's" request for "Rich Aesthetics" (glassmorphism details, smooth transitions).

3.  **Connect to Web Speech API**:
    *   Add `startListening()`, `stopListening()` methods in JS.
    *   Stream results back to Blazor via `DotNetObjectReference`.

### Phase 3: Backend Integration
*   Reuse the `_smartcomponents/smartpaste` endpoint.
*   No changes needed to `SmartComponents.Abstractions` or `SmartComponents.Inference` initially.

## 5. Security & Accessibility

*   **Permissions:** The browser will prompt for Microphone access. We must handle `PermissionDenied` errors gracefully and show a "Microphone access denied" message in the UI.
*   **Anti-forgery:** Ensure the `SmartMicButton` captures the antiforgery token from the parent form just like `SmartPasteButton`.
*   **Keyboard Nav:** The modal must be closable via ESC and navigable via Tab.

## 6. Verification Plan

1.  **Browser Compatibility:** Test in Chrome, Edge, and Safari (Safari often has strict Speech API quirks).
2.  **Fallback:** If Speech API is not available, hide the button or show a specific error.
3.  **Accuracy:** specific test cases for "correcting the text" manually before applying.

## 7. Future Enhancements (Post-MVP)
*   **Server-side Whisper:** For consistent accuracy and wider browser support, streaming audio to the backend (OpenAI Whisper) instead of relying on browser implementation.
*   **Direct-to-Field:** "Voice typing" mode that streams text directly into the focused input, rather than the "Pop-over -> Form Fill" flow.
