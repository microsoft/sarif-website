$(function () {
    var currentIndex = -1;
    var currentColoration = "light";
    var isActive = false;

    // Create the image viewer elements
    $(`
        <div id="imageViewer">
            <div id="imageViewerDialog" role="dialog" aria-labelledby="imageViewerTitle">
                <div id="imageViewerTitleBar">
                    <div id="imageViewerTitle" role="heading" aria-level="1"></div>
                    <button id="imageViewerCloseButton" role="button" aria-label="Close image viewer" tabindex="3">close</button>
                </div>
                <img id="imageViewerImage" role="img" aria-labelledby="imageViewerTitle" />
                <button id="imageViewerPrev" role="button" aria-label="Previous image" tabindex="2" class="image-viewer-button button-left">&#10094;</button>
                <button id="imageViewerNext" role="button" aria-label="Next image" tabindex="1" class="image-viewer-button button-right">&#10095;</button>
            </div>
        </div>
        `).appendTo("body");

    // Add our style sheet
    $("<link href='/css/ImageViewer.css' rel='stylesheet' />").prependTo("head");

    $.fn.enableScroll = function () {
        $(window).off("scroll.scrolldisabler");
    };

    $.fn.disableScroll = function () {
        window.oldScrollPos = $(window).scrollTop();

        $(window).on("scroll.scrolldisabler", function (event) {
            $(window).scrollTop(window.oldScrollPos);
            event.preventDefault();
        });
    };

    $.fn.showImageViewer = function () {
        var sender = $(this)[0];
        var index = Number(sender.getAttribute("index"));

        $("#selector").disableScroll();
        $("#selector").loadImage(index)
        $("#imageViewer").css("display", "flex");
        isActive = true;
    };

    $.fn.loadImage = function (index) {
        var $element = $("#" + imageGallery[index]);
        var imagePath = $element.prop("src").replace(".thumbnail", "");
        var coloration = $element.attr("coloration");

        $("#imageViewerImage").attr("src", imagePath);
        $("#imageViewerTitle").text($element.attr("alt"));

        if (coloration != currentColoration) {
            $(".image-viewer-button").toggleClass("button-invert-colors");
            currentColoration = coloration;
        }

        currentIndex = index;
    };

    $("#imageViewerPrev").on("click", function () {
        $("#selector").loadPreviousImage();
    });

    $("#imageViewerNext").on("click", function () {
        $("#selector").loadNextImage();
    });

    $.fn.loadPreviousImage = function () {
        var index = currentIndex == 0 ? imageGallery.length - 1 : currentIndex - 1;
        $("#selector").loadImage(index);
    };

    $.fn.loadNextImage = function () {
        var index = currentIndex == imageGallery.length - 1 ? 0 : currentIndex + 1;
        $("#selector").loadImage(index);
    };

    $("#imageViewer").on("click", function (event) {
        // Only dismiss if they've clicked outside the dialog area, or the X glyph
        if (event.target.id == "imageViewer" || event.target.id == "imageViewerCloseButton") {
            $("#selector").closeViewer();
        }
    });

    $("body").keyup(function (event) {
        // Dismiss if they've pressed Esc
        if (isActive && event.key == "Escape") {
            $("#selector").closeViewer();
        }
    });

    $.fn.closeViewer = function () {
        $("#selector").enableScroll();
        $("#imageViewer").fadeOut(100);
        isActive = false;
    };
});