//The Comments field should only be visible and editable if the Rating is less than 3. If 
//the rating is 3 or more, keep the Comments field hidden and locked. 

function toggleComments(executionContext) {
    var formContext = executionContext.getFormContext();

    var rating = formContext.getAttribute("hpst_rating").getValue();
    var commentsControl = formContext.getControl("hpst_comments");

    if (!commentsControl) return;

    if (rating !== null && rating < 3) {
        commentsControl.setVisible(true);
        commentsControl.setDisabled(false);
    }
    else {
        commentsControl.setVisible(false);
        commentsControl.setDisabled(true);
        formContext.getAttribute("hpst_comments").setValue(null);
    }
}
//validation 3 Prevent entering comments with forbidden words (simple profanity check).
//SET on validateComments → Comments OnChange validateComments → Form OnSave
function validateComments(executionContext) {
    var formContext = executionContext.getFormContext();

    var comments = formContext.getAttribute("hpst_comments").getValue();

    var forbiddenWords = [
        "stupid",
        "idiot",
        "hate",
        "damn",
        "useless"
    ];

    var control = formContext.getControl("hpst_comments");

    if (!comments) {
        control.clearNotification("ProfanityCheck");
        return;
    }

    var commentText = comments.toLowerCase();

    for (var i = 0; i < forbiddenWords.length; i++) {
        if (commentText.includes(forbiddenWords[i])) {

            control.setNotification(
                "Comments contain inappropriate language.",
                "ProfanityCheck"
            );

            var eventArgs = executionContext.getEventArgs();

            if (eventArgs && typeof eventArgs.preventDefault === "function") {
                eventArgs.preventDefault();
            }

            return;
        }
    }

    control.clearNotification("ProfanityCheck");
}
//Submitted On should remain read-only after creation.
function lockSubmittedOn(executionContext) {
    var formContext = executionContext.getFormContext();

    if (formContext.data.entity.getId()) {
        formContext.getControl("hpst_submittedon").setDisabled(true);
    }
}
//Comments mandatory when Rating is 1 or 2. 
function validateCommentsRequired(executionContext) {
    var formContext = executionContext.getFormContext();

    var rating = formContext.getAttribute("hpst_rating").getValue();
    var commentsAttribute = formContext.getAttribute("hpst_comments");

    if (!commentsAttribute) {
        return;
    }

    if (rating === 1 || rating === 2) {
        commentsAttribute.setRequiredLevel("required");
    }
    else {
        commentsAttribute.setRequiredLevel("none");
    }
}