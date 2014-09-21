$(document).ready(function() {
var $example = $(".example--night"),
		$ceMinutes = $example.find('.ce-minutes'),
		$ceSeconds = $example.find('.ce-seconds'),
		now = new Date(),
		then = new Date(now.getTime() + (23.5*60*60*1000));

	$example.find(".countdown").countEverest({
		second: (then.getSeconds() + 30),
		minute: then.getMinutes(),
		hour: then.getHours(),
		day: then.getDate(),
		month: (then.getMonth()+1),
		year: then.getFullYear()
	})
});
	