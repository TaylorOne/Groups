// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

$(document).ready(function () {
    $("#createPost").submit(function (e) {
        e.preventDefault();

        let form = $(this);

        if ($("#writePost").val() == "") {

        } else {
            $.ajax({
                type: "POST",
                url: form.attr('action'),
                data: {
                    url: window.location.href,
                    postString: $("#writePost").val()
                },
                dataType: "json",
                headers: {
                    RequestVerificationToken:
                        $('input:hidden[name="__RequestVerificationToken"]').val()
                }

            })
                .done(function (json) {
                    console.log(json);
                    $("#posts").prepend(
                        "<div class=\"card mb-3\" style=\"width: 100%;\"><div class=\"card-body\"><h6 class=\"card-title\">" + json["User"] + "</h6><p class=\"card-text\">" + json["Content"] + "</p></div></div>"
                    );
                })
        }

        
    });

    $("#fileUp").submit(function (e) {
        e.preventDefault();

        let formData = new FormData();
        formData.append('FileUpload', $('#file')[0].files[0]);
        formData.append('url', window.location.href);

        $.ajax({
            type: "POST",
            url: "files",
            data: formData,
            processData: false,
            contentType: false,
            dataType: 'json',
            headers: {
                RequestVerificationToken:
                    $('input:hidden[name="__RequestVerificationToken"]').val()
            }

        })
            .done(function (json) {
                console.log(json);
                $("#files").append("<tr><td>" + json["fileName"] + "</td><td>" + json["fileType"] + "</td><td>" + json["dateAdded"] + "</td></tr>");
            })

    });

    $("#imgUp").submit(function (e) {
        e.preventDefault();

        let formData = new FormData();
        formData.append('ImgUpload', $('#file')[0].files[0]);
        formData.append('url', window.location.href);

        $.ajax({
            type: "POST",
            url: "photos",
            data: formData,
            processData: false,
            contentType: false,
            dataType: 'json',
            headers: {
                RequestVerificationToken:
                    $('input:hidden[name="__RequestVerificationToken"]').val()
            }

        })
            .done(function (json) {
                if (json.hasOwnProperty("empty")) {

                } else {
                    $("#imgDisplay").append("<img src=\"http://localhost/images/" + json["imgSafeName"] + "\" class=\"img-thumbnail mr-3 mb-3\" alt=\"" + json["imgUnsafeName"] + "\" />");
                }
            })

    });

    $("#addEvent").submit(function (e) {
        e.preventDefault();

        let form = $(this).serialize();
        form += "&url=" + window.location.href;

        // create JS date objects from form data
        let sD = $("#dateStart").val();
        let eD = $("#dateEnd").val();
        let sT = $("#timeStart").val();
        let eT = $("#timeEnd").val();
        let startDate = Date.parse(sD + "T" + sT);
        let endDate = Date.parse(eD + "T" + eT);

        if ($("#name").val() == "") {
            $("#name").css("border", "1px solid #dc3545");
            $("#nameFieldError").show();
            console.log("You've got error");
        } else if (startDate > endDate) {
            $("#dateEnd").css("border", "1px solid #dc3545");
            $("#timeEnd").css("border", "1px solid #dc3545");
            $("#dateFieldError").show();
        } else {
            $.ajax({
                type: "POST",
                url: "events",
                data: form,
                dataType: "json",
                headers: {
                    RequestVerificationToken:
                        $('input:hidden[name="__RequestVerificationToken"]').val()
                }

            })
                .done(function (json) {
                    let eventElementHTML = "<div class=\"card mb-2\"><div class=\"card-body row\" style=\"padding: 0.75rem\"><div class=\"col-2 border-right\" style=\"text-align: center\" ><b>" + json["month"] + "</b><h2>" + json["day"] + "</h2></div><div class=\"col-10\"><h5 class=\"card-title\">" + json["name"] + "</h5><hr/><div class=\"card-text\"><div class=\"row\"><div class=\"col-7\"><p>" + json["desc"] + "<br></p>Location: " + json["loc"] + "</div><div class=\"col-5\"><p>Start: " + json["startT"] + "<br>End: " + json["endT"] + "</p></div></div></div></div></div></div>";
                    console.log(json);
                    $("#events").append(eventElementHTML);
                    $("#name").css("border", "1px solid #ced4da");
                    $("#dateEnd").css("border", "1px solid #ced4da");
                    $("#timeEnd").css("border", "1px solid #ced4da");
                    $("#nameFieldError").hide();
                    $("#dateFieldError").hide();
                })
        }

    });

});



function JoinGroup(GroupId) {

    $.ajax({
        type: "POST",
        url: "index",
        data: {
            gId: GroupId
        },
        dataType: "json",
        headers: {
            RequestVerificationToken:
                $('input:hidden[name="__RequestVerificationToken"]').val()
        }

    })
        .done(function (json) {
            document.getElementById(GroupId).innerHTML = "joined";
        })
}

function JoinGroupLanding(gId) {

    $.ajax({
        type: "POST",
        url: "landing",
        data: {
            groupID: gId
        },
        dataType: "json",
        headers: {
            RequestVerificationToken:
                $('input:hidden[name="__RequestVerificationToken"]').val()
        }

    })
        .done(function (json) {
            document.getElementById(GroupId).innerHTML = "joined";
        })
}

/*
function AcceptNewMember(GroupId) {

    $.ajax({
        type: "POST",
        url: "members",
        data: {
            Gid: GroupId
        },
        dataType: "json",
        headers: {
            RequestVerificationToken:
                $('input:hidden[name="__RequestVerificationToken"]').val()
        }

    })
        .done(function (json) {
            console.log(json);
        })
}
*/

function AcceptUser(FullName, Id, GroupId) {

    $.ajax({
        type: "POST",
        url: "members",
        data: {
            UserId: Id,
            GroupId: GroupId
        },
        dataType: "json",
        headers: {
            RequestVerificationToken:
                $('input:hidden[name="__RequestVerificationToken"]').val()
        }

    })
        .done(function (json) {
            $("#" + Id).remove();
            var newLi = "<li class=\"list-group-item\"><h6 class=\"card-title\">" + FullName + "</h6></li>";
            $("#members").prepend(newLi);
        })
}

function HighlightJoinButton(elementId) {
    document.getElementById(elementId).style.backgroundColor = "rgba(0,0,0,.07)";
}

function UnHighlightJoinButton(elementId) {
    document.getElementById(elementId).style.backgroundColor = "rgba(0,0,0,.03)";
}

function jg(elementId) {
    document.getElementById(elementId).innerHTML = "joined";
}
