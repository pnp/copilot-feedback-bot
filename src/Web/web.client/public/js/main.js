function initMainJs() {

  initPushyJs();

  jQuery('.pushy').find('li:has(ul)').addClass('pushy-submenu');

  jQuery('.pushy-left ul.menu a').on('click', function () {
    jQuery("body").removeClass("pushy-open-left");
  });

  jQuery('a[href*="#"]:not([href="#"])').click(function () {
    if (location.pathname.replace(/^\//, '') == this.pathname.replace(/^\//, '') && location.hostname == this.hostname) {
      var target = jQuery(this.hash);
      target = target.length ? target : jQuery('[name=' + this.hash.slice(1) + ']');
      if (target.length) {
        jQuery('html, body').animate({
          scrollTop: target.offset().top
        }, 1000);
        return false;
      }
    }
  });

  jQuery("ul#tabs li").click(function (e) {
    if (!jQuery(this).hasClass("active")) {
      var tabNum = jQuery(this).index();
      var nthChild = tabNum + 1;
      jQuery("ul#tabs li.active").removeClass("active");
      jQuery(this).addClass("active");
      jQuery("ul#tab li.active").removeClass("active");
      jQuery("ul#tab li:nth-child(" + nthChild + ")").addClass("active");
    }
  });

  jQuery(document).ready(function (jQuery) {
    jQuery('.accordion-toggle').click(function () {

      //Expand or collapse this panel
      jQuery(this).next().slideToggle('fast');

      //Add active class
      jQuery(this).toggleClass('active');

      //Hide the other panels
      jQuery(".accordion-content").not(jQuery(this).next()).slideUp('fast');

    });
  });

}

function initCharts(quarterChange, monthChange, sevenDayChange) {

  var negativeQuarter = false;
  if (quarterChange < 0) {
    quarterChange = Math.abs(quarterChange);
    negativeQuarter = true;
  }

  var negativeMonth = false;
  if (monthChange < 0) {
    monthChange = Math.abs(monthChange);
    negativeMonth = true;
  }

  var negativeWeek = false;
  if (sevenDayChange < 0) {
    sevenDayChange = Math.abs(sevenDayChange);
    negativeWeek = true;
  }

  Circles.create({
    id: 'overall-circle',
    value: quarterChange,
    radius: 60,
    maxValue: 100,
    width: 5,
    text: function (value) { return (negativeQuarter ? '-' : '') + value.toFixed(0) + '%'; },
    duration: 1200,
    colors: null,
    valueStrokeClass: 'circles-value',
    maxValueStrokeClass: 'circles-bg',
    styleText: false
  });

  Circles.create({
    id: '30day-circle',
    value: monthChange,
    radius: 60,
    maxValue: 100,
    width: 5,
    text: function (value) { return (negativeMonth ? '-' : '') + value.toFixed(0) + '%'; },
    duration: 1200,
    colors: null,
    valueStrokeClass: 'circles-value pink',
    maxValueStrokeClass: 'circles-bg',
    styleText: false
  });

  Circles.create({
    id: '7day-circle',
    value: sevenDayChange,
    radius: 60,
    maxValue: 100,
    width: 5,
    text: function (value) { return (negativeWeek ? '-' : '') + value.toFixed(0) + '%'; },
    duration: 1200,
    colors: null,
    valueStrokeClass: 'circles-value yellow',
    maxValueStrokeClass: 'circles-bg',
    styleText: false
  });
}
