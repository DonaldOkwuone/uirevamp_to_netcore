$.fn.popover508 = function(options) {
	options = options ? options : {};
	options.trigger = options.trigger !== undefined ? options.trigger : "click";
	options.html = options.html !== undefined ? options.html : true;

	// wrap element in a dynamically generated parent element. This is to force the parent to only contain
	// the popover toggle element and the popover, allowing for correct keyboard navigation behavior
	if ($(this).parent("span[data-group=popover-wrapper]").length === 0) {
		$(this).wrap("<span data-group=popover-wrapper></span>");
	}

	var noContainerSpecified = options.container == null;
	// append the popover to the end of the generated parent div for keyboard accessbility
	this.each(function() {
		if (noContainerSpecified) {
			//don't over ride if the caller specifies a container for the popover. 
			options.container = $(this).parent();
		}
		$(this).popover(options);
	});

	$(document).off("blur", "[data-toggle=popover]");
	$(document).off("blur", ".popover-body a");

	$(document).on("blur", "[data-toggle=popover]", function() {
		var popover = this;
		setTimeout(function() {
			var popoverId = $(popover).attr("aria-describedby");
			if (!$(document.activeElement).is($("#" + popoverId).find(".popover-body a"))) {
				(($(popover).popover("hide").data("bs.popover") || {}).inState || {}).click = false;
			}
		}, 100);
	});

	$(document).on("blur", ".popover-body a", function() {
		var popoverLink = this;
		setTimeout(function() {
			var popoverId = $(popoverLink).closest("div[id^=popover]").attr("id");
			if (!$(document.activeElement).is("[data-toggle=popover][aria-describedby=" + popoverId + "]") && !$(document.activeElement).is($("#" + popoverId).find(".popover-body a"))) {
				(($("[data-toggle=popover][aria-describedby=" + popoverId + "]").popover("hide").data("bs.popover") || {}).inState || {}).click = false;
			}
		}, 100);
	});
}