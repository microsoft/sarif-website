$(function () {
    var currentIndex = -1;
    var currentColoration = "light";

    // Create the image viewer elements
    $(`
        <div id="imageViewer">
            <div id="imageViewerDialog">
                <div id="imageViewerTitleBar">
                    <div id="imageViewerTitle" role="banner"></div>
                    <div id="imageViewerCloseGlyph" role="button">close</div>
                </div>
                <img id="imageViewerImage" />
                <div id="imageViewerPrev" role="button" aria-label="Previous image" class="image-viewer-button button-left">&#10094;</div>
                <div id="imageViewerNext" role="button" aria-label="Next image" class="image-viewer-button button-right">&#10095;</div>
            </div>
        </div>
        `).appendTo("body");

    // Add our style sheet
    $("<link href='/Content/ImageViewer.css' rel='stylesheet' />").appendTo("head");

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

    $(".viewer-screenshot").on("click", "img", function () {
        var sender = $(this)[0];
        var index = Number(sender.getAttribute("index"));

        $("#selector").disableScroll();
        $("#selector").loadImage(index)
        $("#imageViewer").css("display", "flex");
    });

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
        if (event.target.id == "imageViewer" || event.target.id == "imageViewerCloseGlyph") {
            $("#selector").enableScroll();
            $("#imageViewer").fadeOut(100);
        }
    });
});