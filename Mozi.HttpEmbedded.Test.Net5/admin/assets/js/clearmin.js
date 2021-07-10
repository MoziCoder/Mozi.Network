window.CM = {
    init: function(config){
	if(typeof config != 'undefined')
	    for(var attr in config)
		this.config[attr] = config[attr];
	if((this.config.restoreMenuState) && ($.cookie('cm-menu-toggled') == 'true'))
	    $('body').addClass('cm-menu-toggled');

	$('body').append('<div id="cm-menu-backdrop" class="visible-xs-block visible-sm-block"></div>' +
			 '<div id="cm-submenu-popover" class="dropdown">' +
			 '<div data-toggle="dropdown"></div>' + 
			 '<div class="popover cm-popover right">' + 
			 '<div class="arrow"></div><div class="popover-content"><ul></ul></div></div></div>');


	this.menu.init();
	this.search.init();
	this.navbars.init();
	this.breadcrumb.init();
	this.tabs.init();
	this.cmPopovers.init();
	this.fixHeight.init();
	if(typeof Touch != 'undefined')
	{
	    FastClick.attach(document.body);
	    if(this.config.menuSwiper)
		this.swiper.init(this);
	}
	var self = this;
	$("[data-toggle='popover']").popover();
	$('.modal').on('show.bs.modal', function(){self.preventScroll.enable();});
	$('.modal').on('hidden.bs.modal', function(){self.preventScroll.disable();});
	$(window).on('load', function(){$('body').removeClass('cm-no-transition')});
    },

    config: {
	'navbarHeight' : 50,
	'menuSwiper' : true,
	'menuSwiperIOS' : true,
	'restoreMenuState' : true
    },

    fixHeight:{
        init: function()
        {
            $(window).on('load', this.process);
            $(window).resize(this.process);
        },
        process: function()
        {
            $('.row.cm-fix-height .panel-body').css('min-height', '0px');
            $('.row.cm-fix-height').each(function(){
                var mh = 0;
                $(this).find('.panel-body').each(function(){
                    var h = $(this).outerHeight();
                    if(h > mh) mh = h;
                }).css('min-height', mh + 'px');
            });
        }
    },

    afterTransition: function(f)
    {
	setTimeout(f, 150);
    },

    breadcrumb: {
        init: function(){	   	   
	    var bc = $('.cm-navbar .breadcrumb');
	    if(bc.length)
	    {
		var c = bc.parent();
		var f = function(){
		    bc.removeClass('lonely');
		    bc.find('li.active.ellipsis').remove();
		    bc.find('li').removeAttr('style');
		    var i = 0;
		    var t = bc.find('li').length - 1;
		    while((bc.outerWidth() > (c.outerWidth() - 15)) && i < t)
		    {
			i++;
			l = bc.find('li:visible:first').hide();
		    }
		    var n = bc.find('li:visible').length;
		    if((n > 1) && i)
			bc.prepend('<li class="active ellipsis">&#133;</li>');
		    else if(n == 1)
		    {
			bc.addClass('lonely');
			bc.find('li:visible:first').width(c.width());
		    }
		};
	    $(window).on('load', f);
	    $(window).resize(f);
	    }
	}
    },

    tabs:{
        init: function(){
	    var noanim = true;
            $('.cm-navbar .nav-tabs li').click(function(){
                var t = $(this);
                var container = t.parent('.nav-tabs');
                var p = container.scrollLeft() + t.position().left;
                var w = t.outerWidth();
                var c = container.width();
                var s = Math.max(p - ((c - w)/2), 0);
		if(noanim)
		{
		    container.scrollLeft(s);
		    noanim = false;
		}
		else
                    container.animate({scrollLeft:s}, 100);
            });
            $('.cm-navbar .nav-tabs').mousewheel(function(e){
                var d = e.deltaY*e.deltaFactor*-1;
                $(this).scrollLeft($(this).scrollLeft() + d);
                return false;
            });
            $('.cm-navbar .nav-tabs li.active').click();
        }
    },

    navbars: {
	init: function(){
	    var self = this;
            var nav = $('.cm-navbar-slideup');
	    var g = $('#global');
            this.l = $(document).scrollTop();
            this.c = 0;
            if(nav.length)
                $(document).scroll(function(){
		    if(!g.hasClass('prevent-scroll'))
		    {
			var o = $(document).scrollTop();
			var s = Math.max(Math.min(self.c-o+self.l, 0), -CM.config.navbarHeight-1);
			if(o <= 0){
                            nav.css('transform', 'translateY(0px)');
                            s = 0;
			}
			else
			{
                            nav.css('transform', 'translateY('+s+'px)');
			}
			self.c = s;
			self.l = o;
		    }
                });
        }
    },

    search: {

	init: function()
	{
	    this.open = $('#cm-search-btn').hasClass('open');
	    this.toggeling = false;
	    var self = this;
	    $("[data-toggle='cm-search']").click(function(){
		if(!self.open && !self.toggeling)
		{
		    self.open = true;
		    $('#cm-search input').focus();
		}
	    });

	    $("[data-toggle='cm-search']").mousedown(function(){
		self.toggeling = self.open;		
	    });

	    $('#cm-search input').focus(function(){
		$('#cm-search').addClass('open');
		$('#cm-search-btn').addClass('open');
		self.open = true;
	    });
	    
	    $('#cm-search input').blur(function(){
		$('#cm-search').removeClass('open');
		CM.afterTransition(function(){
		    $('#cm-search-btn').removeClass('open');
		});
		self.open = false;
	    });
	    

	}

    },

    preventScroll: {
	s: -1,
	enable: function(){
	    this.s = $(document).scrollTop();

	    var f = $('.cm-footer');
	    var x = $(window).height() + this.s - f.position().top - CM.config.navbarHeight;
	    f.css('bottom', x + 'px');
	    $('#global').addClass('prevent-scroll').css('margin-top', '-' + this.s + 'px');
	},
	disable: function(){
	    $("#global").removeAttr('style').removeClass('prevent-scroll');
	    $('.cm-footer').removeAttr('style');
	    if(this.s != -1) $(document).scrollTop(this.s);
	}
    },

    getState: function(){
	var state = {};
	state.mobile = ($('#cm-menu-backdrop').css('display') == 'block');
	state.open = (state.mobile == $('body').hasClass('cm-menu-toggled'));
	return state;
    },

    cmPopovers:{
	init: function(){
	    $('.cm-navbar .popover').each(function(){
                var m = 10;/* minimum doc border space */
                var w = $(this).outerWidth();
                var d = $('body').outerWidth() - m;
                var p = (-w/2) + (CM.config.navbarHeight/2);
                var b = $(this).parent().offset().left + (CM.config.navbarHeight/2);
                var x = b + w/2;
                var y = b - w/2;
                if(x > d)
                {
                    var o = x - d;
                    p-= o;
                    $(this).children('.arrow').css('left', w/2+o);
                }
                else if(y < m)
                {
                    var o = y - m;
                    p-= o;
                    $(this).children('.arrow').css('left', w/2+o);
                }
                $(this).css('left', p);
            });
	}
    },

    menu: {
	init: function(){
	    var scroll = 0;
	    var self = this;

	    $('.cm-submenu ul').click(function(e){e.stopPropagation()});
	    $('#cm-menu-scroller').scroll(this.hidePopover);
	    $("[data-toggle='cm-menu']").click(this.toggle);
	    $('#cm-menu-backdrop').click(function(){$('body').removeClass('cm-menu-toggled')});
	    $("#cm-menu-scroller").mousewheel(function(e){
		var n = CM.config.navbarHeight + 1;
		var s = e.deltaY * n + scroll;
		var max = -$(window).height() + n;
		$('.cm-menu-items > li, .cm-submenu.open > ul').each(function(){
		    max+= $(this).height();
		});
		s = Math.max(Math.min(s, 0), -n * Math.ceil(max / n));
		s = Math.min(s, 0);
		$('.cm-menu-items').css('transform', 'translateY(' + s + 'px)');
		scroll = s;
		self.hidePopover();
		return false;
	    });

	    $('#cm-menu a').click(function(){
		var state = CM.getState();
		var href = $(this).attr('href');
		if(href)
		{
		    if(state.mobile)
		    {
			$('body').removeClass('cm-menu-toggled');
			$.cookie('cm-menu-toggled', false, {path:'/'});
		    }		    
		    if(!$(this).parents('.cm-submenu').length)
		    {
			$('.cm-menu-items li').removeAttr('style');
			$('.cm-submenu').removeClass('open');
		    }
		}
	    });

	    $('.cm-submenu').click(function(e, notrans, nopopo){
		var m = $(this);
		var state = CM.getState();
		if((!state.mobile) && (!state.open))
		{
		    self.setPopover(m);
		    return false;
		}
		var open = m.hasClass('open');
		$('.cm-submenu').removeClass('open');
		$('.cm-menu-items li').removeAttr('style');
		if(!open)
		{
		    m.addClass('open');
		    m.nextAll().css('transform', 'translateY(' + m.children('ul').height() + 'px)');
		}
	    });

	    var state = CM.getState();
	    if((!state.mobile) && (!state.open))
	    {
		$('.cm-submenu.pre-open').removeClass('pre-open');
	    }
	    else
	    {
		var po = $('.cm-submenu.pre-open');
		po.nextAll().css('transform', 'translateY(' + po.children('ul').height() + 'px)');
		po.addClass('open').removeClass('pre-open');
	    }

	},

	hidePopover: function()
	{
	    $('#cm-submenu-popover').removeClass('open');
	},

	setPopover: function(li)
	{
	    var p = $('#cm-submenu-popover');
	    var open = p.hasClass('open');
	    var popen = li.hasClass('popen');
	    $('.cm-submenu').removeClass('popen');
	    if(popen && open){this.hidePopover();return true;}
	    $('#cm-submenu-popover ul').html(li.find('ul').html());
	    var m = 10;
	    var d = $(window).height() - m;
	    var a = $('#cm-submenu-popover').find('.arrow');
	    var h = p.find('.popover').height();
	    var y = li.position().top + CM.config.navbarHeight*1.5 - h/2;
	    var x = y + h;
            a.show();
	    if(x > d){
                var o = x - d;
                y-= o;
                a.css('top', h/2+o);
            }
            else if(y < m){
		var o = y - m;
                y-= o;
                a.css('top', h/2+o);
            }
            else{
                a.css('top', '50%');
            }
            if(a.position().top > h){
                a.hide();
            }
	    p.css('top', y);
	    li.addClass('popen');	    
	    if(!open)
		$("#cm-submenu-popover [data-toggle='dropdown']").click();
	},

	toggle: function(){
	    $(".container-fluid").addClass('animate');
	    $("body").toggleClass("cm-menu-toggled");
	    var state = CM.getState();
	    if(!state.mobile){
		$('.cm-submenu').removeClass('open');
		$('.cm-menu-items li').removeAttr('style');
		$(window).resize();
		$.cookie('cm-menu-toggled', (!state.open), {path:'/'});
            } else {
		$.cookie('cm-menu-toggled', false, {path:'/'});
		state.open ? CM.preventScroll.enable() : CM.preventScroll.disable();
	    }
	}
    },

    swiper:{
	init: function(){
	    var self = this;
	    this.lock = false;
            this.menu = $('#cm-menu');
            this.mask = $('#cm-menu-backdrop');
            this.mwidth = this.menu.width();
	    this.ios = navigator.vendor.indexOf("Apple")==0 && /\sSafari\//.test(navigator.userAgent);
	    if(this.ios && (!CM.config.menuSwiperIOS)) return false;
	    var triggers = $("[data-toggle='cm-menu']");
	    $(triggers).bind('touchstart', function(e){$(this).addClass('active'); return false;});
	    $(triggers).bind('touchmove', function(e){return false;});
	    $(triggers).bind('touchend', function(e){$(this).removeClass('active');$(this).click(); return false;});
	    $(triggers).bind('touchcancel', function(e){$(this).removeClass('active');$(this).click(); return false;});
	    $(document).bind('touchstart', function(e){return self.start(e);});
            $(document).bind('touchmove', function(e){return self.move(e);});
            $(document).bind('touchend', function(e){return self.end(e);});
            $(document).bind('touchcancel', function(e){return self.end(e);});
	},

	start: function(e)
	{
	    this.threshold = false;
	    var touch = e.originalEvent.changedTouches[0];
	    var openMinPos = this.ios ? 10 : 0;
	    var openMaxPos = this.ios ? 90 : 50;
	    this.lt = Date.now();
	    this.lx = touch.clientX;
	    this.mobile = (this.mask.css('display') == 'block');
	    this.open = (this.mobile == $('body').hasClass('cm-menu-toggled'));
	    this.xStart = touch.clientX;
	    this.yStart = touch.clientY;
	    this.lock = ((this.mobile && !this.open && ((this.xStart > openMaxPos) || (this.xStart < openMinPos))) ||
			 (!this.mobile))
	    if(this.mobile && this.open)
		this.xStart = Math.min(this.xStart, this.mwidth);
	    if(!this.lock){
		$('body').addClass('cm-no-transition');
	    }
	    return true;
	},

	move: function(e)
	{
	    var touch = e.originalEvent.changedTouches[0];
	    var dy = touch.clientY - this.yStart;
	    var t = Date.now();
	    this.m = Math.abs(touch.clientX - this.lx)/(t - this.lt);
	    this.lx = touch.clientX;
	    this.lt = t;
	    this.dx = touch.clientX - this.xStart;
	    if((Math.abs(this.dx) < 10) && (!this.threshold)){
		this.dx = 0;
	    } else {
		this.threshold = true;
	    }
	    if((Math.abs(dy) > (Math.abs(this.dx)*2)) || this.lock)
		return true;
	    if(this.mobile && this.open)
	    {
		var x = Math.min(this.mwidth + this.dx, this.mwidth);
		this.translate(this.menu, x);
		this.mask.css('opacity', (x/this.mwidth)/2);
	    }
	    else if(this.mobile && !this.open)
	    {
		var x = Math.min(this.dx + this.xStart, this.mwidth);
		this.translate(this.menu, x);
		this.mask.css('visibility', 'visible');
		this.mask.css('opacity', (x/this.mwidth)/2);
	    }
	    return true;
	},

	end: function(e)
	{
	    if(this.lock){
		return true;
	    }
	    $('body').removeClass('cm-no-transition');
	    var z = Math.min(Math.max(this.m,1),3)*(this.open?-1:1)*this.dx*2;
	    if(z > this.mwidth)
		CM.menu.toggle();
	    this.menu.removeAttr('style');
            this.mask.removeAttr('style');
	    return true;
	},
	
	translate: function(o, x){
	    o.css('transform', 'translateX(' + x + 'px)');
	}
    }
};

$(function(){CM.init({
    'navbarHeight' : 50,
    'menuSwiper' : true,
    'menuSwiperIOS' : true,
    'restoreMenuState' : true
})});
