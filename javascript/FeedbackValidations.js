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