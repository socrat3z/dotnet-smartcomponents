# Smart Mic

Smart Mic is an intelligent voice input feature that fills out forms automatically using speech recognition. It allows users to dictate information, which is then parsed and distributed to the correct form fields.

![Screen capture of Smart Mic feature](images/smart-mic-demo.gif)

### Example use cases

 * **Field Reporting**
 
   A technician inspecting equipment can dictate their findings ("The pump is vibrating excessively, recommending immediate maintenance on unit 4B") directly into the maintenance log form, populating fields like "Status", "Description", "Urgency", and "Unit ID".

 * **Medical dictation**
 
   A doctor or nurse can speak patient notes ("Patient reports mild headache since Tuesday, no fever, blood pressure 120 over 80") to fill out the intake form, mapping to "Symptoms", "Onset Date", "Vitals", etc.

 * **On-the-go CRM updates**
 
   Sales representatives can dictate meeting summaries while walking to their next appointment, updating deal stages and next steps without typing on a mobile keyboard.

Smart Mic uses the browser's built-in speech recognition for immediate transcription, followed by the same powerful language model as [Smart Paste](smart-paste.md) to understand the context and structure of your form.

## Adding Smart Mic in Blazor

First, ensure you have completed the [Smart Component installation steps for Blazor](getting-started-blazor.md) and [configured an OpenAI backend](configure-openai-backend.md).

Then, in a `.razor` file, inside any `<form>` or `<EditForm>`, add the `<SmartMicButton>` component. Example:

```razor
@page "/field-report"
@using SmartComponents

<EditForm Model="@report" OnValidSubmit="SubmitReport">
    <div class="mb-3">
        <label>Unit ID</label>
        <InputText @bind-Value="report.UnitId" class="form-control" />
    </div>
    <div class="mb-3">
        <label>Description</label>
        <InputTextArea @bind-Value="report.Description" class="form-control" />
    </div>
    <div class="mb-3">
        <label>Status</label>
        <InputSelect @bind-Value="report.Status" class="form-control">
            <option value="">Select...</option>
            <option value="Operational">Operational</option>
            <option value="Warning">Warning</option>
            <option value="Critical">Critical</option>
        </InputSelect>
    </div>

    <button type="submit" class="btn btn-primary">Submit Report</button>
    <SmartMicButton DefaultIcon />
</EditForm>

@code {
    ReportModel report = new();
    
    class ReportModel {
        public string? UnitId { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
    }
}
```

When the user clicks the microphone button:
1.  A dialog appears, listening for speech.
2.  The spoken words are transcribed in real-time.
3.   The user can review and edit the text if needed.
4.  Clicking "Apply" sends the text to the backend, which intelligently fills the form fields.

## Adding Smart Mic in MVC / Razor Pages

Ensure you follow the [installation steps for MVC/Razor Pages](getting-started-mvc-razor-pages.md).

In a `.cshtml` file, add the `<smart-mic-button>` tag inside a `<form>`:

```cshtml
<form method="post">
    <div class="form-group">
        <label>Unit ID</label>
        <input name="UnitId" class="form-control" />
    </div>
    <div class="form-group">
        <label>Description</label>
        <textarea name="Description" class="form-control"></textarea>
    </div>
    
    <button type="submit" class="btn btn-primary">Save</button>
    <smart-mic-button default-icon />
</form>
```

## Customizing the button

The `<SmartMicButton>` renders as a standard HTML `<button>` element. You can style it using standard CSS classes.

### Default Styles
The button automatically receives the class `smart-mic-button`. While listening or processing, it may toggle states that you can target with CSS.

```css
/* Custom style for the mic button */
.smart-mic-button {
    background-color: transparent;
    border: 1px solid #ccc;
    border-radius: 50%;
    width: 40px;
    height: 40px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
}

.smart-mic-button:hover {
    background-color: #f0f0f0;
}
```

### Custom Content
To use a custom icon or label, provide child content instead of using `DefaultIcon`:

```razor
<SmartMicButton class="btn btn-outline-secondary">
    <i class="fas fa-microphone"></i> Dictate
</SmartMicButton>
```

## Privacy & Permissions

Smart Mic uses the **Web Speech API**, which may require sending audio data to a third-party service (like Google's servers for Chrome) for transcription. 

*   **Browser Prompt:** The first time a user clicks the button, the browser will ask for permission to use the microphone.
*   **Secure Context:** This feature typically requires the site to be served over HTTPS (or localhost).
*   **Handling Denials:** If the user denies permission, the button will simply not activate the listening mode. You may want to provide UI hints or instructions if microphone access is critical for your app.
