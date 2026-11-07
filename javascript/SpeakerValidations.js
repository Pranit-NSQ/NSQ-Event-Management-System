var NSQ = window.NSQ || {};
NSQ.Speaker = NSQ.Speaker || {};

NSQ.Speaker.validateEmailAndPhoneOnSave = function (executionContext) {
  var formContext = executionContext.getFormContext();
  var eventArgs = executionContext.getEventArgs();

  var emailAttribute = formContext.getAttribute("hpst_email");
  var phoneAttribute = formContext.getAttribute("hpst_phonenumber");

  var isValid = true;

  // Clear previous validation messages.
  emailAttribute.controls.forEach(function (control) {
    control.clearNotification("speaker_email_format");
  });

  phoneAttribute.controls.forEach(function (control) {
    control.clearNotification("speaker_phone_format");
  });

  var email = emailAttribute.getValue();
  var phone = phoneAttribute.getValue();

  // Validate Email format when Email is provided.
  if (email !== null && email.trim() !== "") {
    var emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    if (!emailPattern.test(email.trim())) {
      emailAttribute.controls.forEach(function (control) {
        control.setNotification(
          "Enter a valid Email address, for example abc@example.com.",
          "speaker_email_format",
        );
      });

      isValid = false;
    }
  }

  // Validate Phone Number when Phone Number is provided.
  if (phone !== null && phone.trim() !== "") {
    var phonePattern = /^\+[0-9]+$/;

    if (!phonePattern.test(phone.trim())) {
      phoneAttribute.controls.forEach(function (control) {
        control.setNotification(
          "Phone Number must start with + and contain digits only, for example +919876543210.",
          "speaker_phone_format",
        );
      });

      isValid = false;
    }
  }

  // Prevent saving if either provided value is invalid.
  if (!isValid) {
    eventArgs.preventDefault();
  }
};

// ============================================================

// Validation 3: Validate LinkedIn Profile URL
// Event: Speaker Main Form - OnSave
// Purpose: Validate the LinkedIn profile URL when provided.

// ============================================================

NSQ.Speaker.validateLinkedInProfileUrlOnSave = function (executionContext) {
  var formContext = executionContext.getFormContext();

  var eventArgs = executionContext.getEventArgs();

  var linkedinAttribute = formContext.getAttribute("hpst_linkedinprofile");

  // Stop if the field is not available on this form.

  if (!linkedinAttribute) {
    return;
  }

  // Clear the previous validation notification.

  linkedinAttribute.controls.forEach(function (control) {
    control.clearNotification("speaker_linkedin_url");
  });

  var linkedinUrl = linkedinAttribute.getValue();

  // LinkedIn Profile is optional, so allow an empty value.

  if (linkedinUrl === null || linkedinUrl.trim() === "") {
    return;
  }

  var isValid = false;

  // Validate URL protocol, domain and LinkedIn profile path.

  try {
    var parsedUrl = new URL(linkedinUrl.trim());

    var validDomain =
      parsedUrl.hostname.toLowerCase() === "linkedin.com" ||
      parsedUrl.hostname.toLowerCase() === "www.linkedin.com";

    var validProfilePath = /^\/in\/[^/]+\/?$/i.test(parsedUrl.pathname);

    isValid =
      parsedUrl.protocol === "https:" && validDomain && validProfilePath;
  } catch (error) {
    isValid = false;
  }

  // Display an error and prevent saving when URL is invalid.

  if (!isValid) {
    linkedinAttribute.controls.forEach(function (control) {
      control.setNotification(
        "Enter a valid LinkedIn profile URL, for example https://www.linkedin.com/in/your-name/.",

        "speaker_linkedin_url",
      );
    });

    eventArgs.preventDefault();
  }
};
