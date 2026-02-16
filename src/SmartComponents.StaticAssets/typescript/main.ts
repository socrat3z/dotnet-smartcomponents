import { registerSmartComboBoxCustomElement } from './SmartComboBox';
import { registerSmartPasteClickHandler } from './SmartPaste';
import { registerSmartMicClickHandler } from './SmartMic';
import { registerSmartTextAreaCustomElement } from './SmartTextArea/SmartTextArea';
import './SmartTranslate';

// Only run this script once. If you import it multiple times, the 2nd-and-later are no-ops.
const isLoadedMarker = '__smart_components_loaded__';
if (!Object.getOwnPropertyDescriptor(document, isLoadedMarker)) {
    Object.defineProperty(document, isLoadedMarker, { enumerable: false, writable: false });

    registerSmartComboBoxCustomElement();
    registerSmartPasteClickHandler();
    registerSmartMicClickHandler();
    registerSmartTextAreaCustomElement();
    // SmartTranslate handles its own global export in its file, 
    // but we need to import it here so it's included in the bundle.
    import('./SmartTranslate');
}
