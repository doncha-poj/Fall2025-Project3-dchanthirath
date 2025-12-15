/**
 * Custom global client-side scripts for Fall2025-Project3-dchanthirath
 */

$(document).ready(function () {
    // Initialize custom scripts for the ActorMovie Create view
    setupActorMovieForm();
});

/**
 * Handles the form submission for the ActorMovie Create view using AJAX.
 * This function sends the data to the server without a full page reload.
 * * @param {FormData} formData - The data from the ActorMovie form.
 */
function createActorMovieRelationship(formData) {
    // Get the Anti-Forgery Token from the form
    const antiForgeryToken = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: '/ActorMovies/Create', // The target controller action
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        headers: {
            // Include the Anti-Forgery Token in the request header
            'RequestVerificationToken': antiForgeryToken
        },
        success: function (response) {
            // Assuming the controller returns a JSON object on success
            if (response && response.success) {
                // Example of success message/action
                console.log('Actor-Movie relationship created successfully.', response.newRole);
                alert('Role "' + response.newRole.role + '" added successfully!');

                // Clear the form fields for the next entry
                $('#actorMovieForm')[0].reset();

                // OPTIONAL: Update a list on the page to show the new entry
                // You would need to check your specific ViewModel structure to implement this.
            } else {
                // Handle server-side validation or failure messages returned in JSON
                console.error('Error creating relationship:', response.message);
                alert('Error: ' + (response.message || 'Failed to add role. Please check your data.'));
            }
        },
        error: function (xhr, status, error) {
            // Handle general AJAX errors (e.g., network, 500 server error)
            console.error('AJAX Error:', error);
            alert('An unexpected error occurred. Please try again.');
        }
    });
}

/**
 * Sets up the event listener for the ActorMovie creation form.
 */
function setupActorMovieForm() {
    // Check if the form exists on the current page before binding the event
    const form = document.getElementById('actorMovieForm');

    if (form) {
        $(form).on('submit', function (e) {
            e.preventDefault(); // Prevent the default form submission (full page reload)

            // Create a FormData object from the form for easy handling of all fields
            const formData = new FormData(this);

            // Perform basic client-side validation before sending
            if ($(this).valid()) {
                createActorMovieRelationship(formData);
            } else {
                console.log('Client-side validation failed.');
            }
        });
    }
}