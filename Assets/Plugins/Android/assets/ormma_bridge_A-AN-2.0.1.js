/*  Copyright (c) 2011 The ORMMA.org project authors. All Rights Reserved.
 *
 *  Use of this source code is governed by a BSD-style license
 *  that can be found in the LICENSE file in the root of the source
 *  tree. All contributing project authors may
 *  be found in the AUTHORS file in the root of the source tree.
 */

(function() {

	var ormmaview = window.ormmaview = {};
	var vserv = window.vserv = {};

	/** ************************************************* */
	/** ******** PROPERTIES OF THE ORMMA BRIDGE ********* */
	/** ************************************************* */

	/** Expand Properties */
	var expandProperties = {
		useBackground : false,
		backgroundColor : '#ffffff',
		backgroundOpacity : 1.0,
		lockOrientation : false
	};

	/** The set of listeners for ORMMA Native Bridge Events */
	var listeners = {};

	/** Holds the current dimension values */
	dimensions: {
	}
	;

	/** A Queue of Calls to the Native SDK that still need execution */
	var nativeCallQueue = [];

	/** Identifies if a native call is currently in progress */
	var nativeCallInFlight = false;

	/** timer for identifying iframes */
	var timer;
	var totalTime;

	/** ******************************************* */
	/** *********** JAVA ENTRY POINTS ************* */
	/** ******************************************* */

	/**
	 * Called by the JAVA SDK when an asset has been fully cached.
	 * 
	 * @returns string, "OK"
	 */
	ormmaview.fireAssetReadyEvent = function(alias, URL) {

		var handlers = listeners["assetReady"];
		if (handlers != null) {
			for ( var i = 0; i < handlers.length; i++) {
				handlers[i](alias, URL);
			}
		}

		return "OK";
	};

	/**
	 * Called by the ..... SDK when an asset has been removed from the cache at
	 * the request of the creative.
	 * 
	 * @returns string, "OK"
	 */
	ormmaview.fireAssetRemovedEvent = function(alias) {
		// console.log("inside ormmaview.fireAssetRemovedEvent from
		// ormma_bridge.js:");
		var handlers = listeners["assetRemoved"];
		if (handlers != null) {
			for ( var i = 0; i < handlers.length; i++) {
				handlers[i](alias);
			}
		}

		return "OK";
	};

	/**
	 * Called by the JAVA SDK when an asset has been automatically removed from
	 * the cache for reasons outside the control of the creative.
	 * 
	 * @returns string, "OK"
	 */
	ormmaview.fireAssetRetiredEvent = function(alias) {
		// console.log("inside ormmaview.fireAssetRetiredEvent from
		// ormma_bridge.js:");
		var handlers = listeners["assetRetired"];
		if (handlers != null) {
			for ( var i = 0; i < handlers.length; i++) {
				handlers[i](alias);
			}
		}

		return "OK";
	};

	/**
	 * Called by the JAVA SDK when various state properties have changed.
	 * 
	 * @returns string, "OK"
	 */
	ormmaview.fireChangeEvent = function(properties) {
		// console.log("inside ormmaview.fireChangeEvent from
		// ormma_bridge.js:properties:"+properties);
		var handlers = listeners["change"];
		if (handlers != null) {
			// console.log("inside ormmaview.fireChangeEvent from
			// ormma_bridge.js:handlers != null");

			for ( var i = 0; i < handlers.length; i++) {
				// console.log("**inside ormmaview.fireChangeEvent from
				// ormma_bridge.js:"+properties+" handlers[i]:"+handlers[i]);
				handlers[i](properties);
			}
		}

		return "OK";
	};

	/**
	 * Called by the JAVA SDK when an error has occured.
	 * 
	 * @returns string, "OK"
	 */
	ormmaview.fireErrorEvent = function(message, action) {
		// console.log("inside ormmaview.fireErrorEvent from ormma_bridge.js:");
		var handlers = listeners["error"];
		if (handlers != null) {
			for ( var i = 0; i < handlers.length; i++) {
				handlers[i](message, action);
			}
		}

		return "OK";
	};

	/**
	 * Called by the JAVA SDK when the user shakes the device.
	 * 
	 * @returns string, "OK"
	 */
	ormmaview.fireShakeEvent = function() {
		// console.log("inside ormmaview.fireShakeEvent from ormma_bridge.js:");
		var handlers = listeners["shake"];
		if (handlers != null) {
			for ( var i = 0; i < handlers.length; i++) {
				handlers[i]();
			}
		}

		return "OK";
	};

	/**
	 * 
	 */
	ormmaview.showAlert = function(message) {
		ORMMAUtilityControllerBridge.showAlert(message);
	};

	/** ****************************************** */
	/** ******** INTERNALLY USED METHODS ********* */
	/** ****************************************** */

	/**
	 * 
	 */
	ormmaview.zeroPad = function(number) {
		var text = "";
		if (number < 10) {
			text += "0";
		}
		text += number;
		return text;
	}

	/** ************************************************************************ */
	/** ******** LEVEL 0 (not part of spec, but required by public API ********* */
	/** ************************************************************************ */

	/**
	 * 
	 */
	ormmaview.activate = function(event) {
		// console.log("inside ormmaview.activate from ormma_bridge.js:");
		ORMMAUtilityControllerBridge.activate(event);
	};

	/**
	 * 
	 */
	ormmaview.addEventListener = function(event, listener) {
		// console.log("**inside ormmaview.addEventListener from
		// ormma_bridge.js:event:"+event+" listeners[event]:"+listeners[event]);
		var handlers = listeners[event];
		if (handlers == null) {
			// no handlers defined yet, set it up
			// console.log("no handlers defined yet, set it up");
			listeners[event] = [];
			handlers = listeners[event];
		}

		// see if the listener is already present
		for ( var handler in handlers) {
			if (listener == handler) {
				// console.log("listener already present, nothing to do");
				// listener already present, nothing to do
				return;
			}
		}

		// not present yet, go ahead and add it
		handlers.push(listener);
	};

	/**
	 * 
	 */
	ormmaview.deactivate = function(event) {
		// console.log("inside ormmaview.deactivate from ormma_bridge.js:");
		ORMMAUtilityControllerBridge.deactivate(event);
	};

	/**
	 * 
	 */
	ormmaview.removeEventListener = function(event, listener) {
		// console.log("inside ormmaview.removeEventListener from
		// ormma_bridge.js:");
		var handlers = listeners[event];
		if (handlers != null) {
			handlers.remove(listener);
		}
	};

	/** ************************** */
	/** ******** LEVEL 1 ********* */
	/** ************************** */

	/**
	 * 
	 */
	ormmaview.close = function() {
		// console.log("inside ormmaview.close from ormma_bridge.js:");
		try {
			ORMMADisplayControllerBridge.close();
		} catch (e) {
			ormmaview.showAlert("close: " + e);
		}
	};

	/**
	 * 
	 */
	ormmaview.expand = function(dimensions, URL) {
		// console.log("inside ormmaview.expand from ormma_bridge.js:");
		try {
			this.dimensions = dimensions;
			ORMMADisplayControllerBridge.expand(
					ormmaview.stringify(dimensions), URL, ormmaview
							.stringify(expandProperties));
		} catch (e) {
			ormmaview.showAlert("executeNativeExpand: " + e + ", dimensions = "
					+ dimensions + ", URL = " + URL + ", expandProperties = "
					+ expandProperties);
		}
	};

	/**
	 * 
	 */
	ormmaview.hide = function() {
		// console.log("inside ormmaview.hide from ormma_bridge.js:");
		try {
			ORMMADisplayControllerBridge.hide();
		} catch (e) {
			ormmaview.showAlert("hide: " + e);
		}
	};

	/**
	 * 
	 */
	ormmaview.open = function(URL, controls) {
		console.log("inside ormmaview.open");

		var back = false;
		var forward = false;
		var refresh = false;
		if (controls == null) {
			back = true;
			forward = true;
			refresh = true;
		} else {
			for ( var i = 0; i < controls.length; i++) {
				if ((controls[i] == "none") && (i > 0)) {
					// error
					self
							.fireErrorEvent(
									"none must be the only navigation element present.",
									"open");
					return;
				} else if (controls[i] == "all") {
					if (i > 0) {
						// error
						self
								.fireErrorEvent(
										"none must be the only navigation element present.",
										"open");
						return;
					}

					// ok
					back = true;
					forward = true;
					refresh = true;
				} else if (controls[i] == "back") {
					back = true;
				} else if (controls[i] == "forward") {
					forward = true;
				} else if (controls[i] == "refresh") {
					refresh = true;
				}
			}
		}

		try {
			console.log("ORMMADisplayControllerBridge.open");
			ORMMADisplayControllerBridge.open(URL, back, forward, refresh);
			recordClicks("ormma.open(" + URL + "," + controls + ")");

		} catch (e) {

		}

	};

	/**
	 * 
	 */
	ormmaview.openMap = function(POI, fullscreen) {
		// console.log("inside ormmaview.openMap from ormma_bridge.js:");
		try {
			ORMMADisplayControllerBridge.openMap(POI, fullscreen);
			recordClicks("ormma.openMap(" + POI + "," + fullscreen + ")");
		} catch (e) {
			ormmaview.showAlert("openMap: " + e);
		}
	};

	/**
	 * 
	 */
	ormmaview.playAudio = function(URL, properties) {

		var autoPlay = false, controls = false, loop = false, position = false, startStyle = 'normal', stopStyle = 'normal';

		if (properties != null) {

			if ((typeof properties.autoplay != "undefined")
					&& (properties.autoplay != null)) {
				autoPlay = true;
			}

			if ((typeof properties.controls != "undefined")
					&& (properties.controls != null)) {
				controls = true;
			}

			if ((typeof properties.loop != "undefined")
					&& (properties.loop != null)) {
				loop = true;
			}

			if ((typeof properties.position != "undefined")
					&& (properties.position != null)) {
				position = true;
			}

			// TODO check valid values...

			if ((typeof properties.startStyle != "undefined")
					&& (properties.startStyle != null)) {
				startStyle = properties.startStyle;
			}

			if ((typeof properties.stopStyle != "undefined")
					&& (properties.stopStyle != null)) {
				stopStyle = properties.stopStyle;
			}

			if (startStyle == 'normal') {
				position = true;
			}

			if (position) {
				autoPlay = true;
				controls = false;
				loop = false;
				stopStyle = 'exit';
			}

			if (loop) {
				stopStyle = 'normal';
				controls = true;
			}

			if (!autoPlay) {
				controls = true;
			}

			if (!controls) {
				stopStyle = 'exit';
			}
		}

		try {
			recordClicks("ormma.playAudio(" + URL + "," + properties + ")");
			ORMMADisplayControllerBridge.playAudio(URL, autoPlay, controls,
					loop, position, startStyle, stopStyle);
		} catch (e) {
			ormmaview.showAlert("playAudio: " + e);
		}
	};

	ormmaview.playVideo = function(URL, properties) {
		console.log("inside ormmaview.playVideo from ormma_bridge.js:");

		var audioMuted = false, autoPlay = false, controls = false, loop = false, position = [
				-1, -1, -1, -1 ], startStyle = 'normal', stopStyle = 'normal';
		if (properties != null) {

			if ((typeof properties.audio != "undefined")
					&& (properties.audio != null)) {
				audioMuted = properties.audio;
			}

			if ((typeof properties.autoplay != "undefined")
					&& (properties.autoplay != null)) {
				autoPlay = properties.autoplay;
			}

			if ((typeof properties.controls != "undefined")
					&& (properties.controls != null)) {
				controls = properties.controls;
			}

			if ((typeof properties.loop != "undefined")
					&& (properties.loop != null)) {
				loop = properties.loop;
			}

			if ((typeof properties.position != "undefined")
					&& (properties.position != null)) {
				position[0] = properties.position.top;
				position[1] = properties.position.left;
				position[2] = properties.width;
				position[3] = properties.height;
			}

			if ((typeof properties.startStyle != "undefined")
					&& (properties.startStyle != null)) {
				startStyle = properties.startStyle;
			}

			if ((typeof properties.stopStyle != "undefined")
					&& (properties.stopStyle != null)) {
				stopStyle = properties.stopStyle;
			}

			if (loop) {
				stopStyle = 'normal';
				controls = true;
			}

			if (!autoPlay)
				controls = true;

			if (!controls) {
				stopStyle = 'exit';
			}

			if (position[0] == -1 || position[1] == -1) {
				startStyle = "fullscreen";
			}
		}

		try {
			clearPostActionNotifyUrl();
			ORMMADisplayControllerBridge.playVideo(URL, audioMuted, autoPlay,
					controls, loop, position, startStyle, stopStyle);
			recordClicks("ormma.playVideo(" + URL + "," + properties + ")");

		} catch (e) {
			ormmaview.showAlert("playVideo: " + e);
		}

	};

	/**
	 * 
	 */
	ormmaview.resize = function(width, height) {
		// console.log("inside ormmaview.resize from ormma_bridge.js:");
		try {
			ORMMADisplayControllerBridge.resize(width, height);
		} catch (e) {
			ormmaview.showAlert("resize: " + e);
		}
	};

	ormmaview.getExpandProperties = function() {
		// console.log("inside ormmaview.getExpandProperties from
		// ormma_bridge.js:");
		return expandProperties;
	}

	/**
	 * 
	 */
	ormmaview.setExpandProperties = function(properties) {
		// console.log("inside ormmaview.setExpandProperties from
		// ormma_bridge.js:");
		expandProperties = properties;
	};

	/**
	 * 
	 */
	ormmaview.show = function() {
		// console.log("inside ormmaview.show from ormma_bridge.js:");
		try {
			ORMMADisplayControllerBridge.show();
		} catch (e) {
			ormmaview.showAlert("show: " + e);
		}
	};

	/** ************************** */
	/** ******** LEVEL 2 ********* */
	/** ************************** */

	/**
	 * 
	 */
	ormmaview.createEvent = function(date, eventTitle, eventDetails) {
		console.log("inside ormmaview.createEvent from ormma_bridge.js:");

		try {
			console.log("Inside try block ");
			ORMMAUtilityControllerBridge.createEvent(date, eventTitle,
					eventDetails);
			console.log("After create event");
			recordClicks("ormma.createEvent(" + date + "," + eventTitle + ","
					+ eventDetails + ")");

		} catch (e) {

			ormmaview.showAlert("createEvent(" + date + "," + eventTitle + ","
					+ eventDetails + ")");

		}

	};

	/**
	 * 
	 */
	ormmaview.makeCall = function(phoneNumber) {
		// console.log("inside ormmaview.makeCall from ormma_bridge.js:");
		try {
			// if(isPreviousClickPassedSafeTime() == true){
			ORMMAUtilityControllerBridge.makeCall(phoneNumber);
			recordClicks("ormma.makeCall(" + phoneNumber + ")");
			// }
		} catch (e) {
			ormmaview.showAlert("makeCall: " + e);
		}
	};

	/**
	 * 
	 */
	ormmaview.sendMail = function(recipient, subject, body) {
		// console.log("inside ormmaview.sendMail from ormma_bridge.js:");
		try {
			// if(isPreviousClickPassedSafeTime() == true){
			ORMMAUtilityControllerBridge.sendMail(recipient, subject, body);
			recordClicks("ormma.sendMail(" + recipient + "," + subject + ","
					+ body + ")");
			// }
		} catch (e) {
			ormmaview.showAlert("sendMail: " + e);
		}
	};

	ormmaview.sendSMS = function(recipient, body) {
		// console.log("inside ormmaview.sendSMS from ormma_bridge.js:");
		try {
			// if(isPreviousClickPassedSafeTime() == true){
			ORMMAUtilityControllerBridge.sendSMS(recipient, body);
			recordClicks("ormma.sendSMS(" + recipient + "," + body + ")");
			// }
		} catch (e) {
			ormmaview.showAlert("sendSMS: " + e);
		}
	};

	ormmaview.setShakeProperties = function(properties) {
	};

	/** ************************** */
	/** ******** LEVEL 3 ********* */
	/** ************************** */

	/**
	 * 
	 */
	ormmaview.addAsset = function(URL, alias) {

	};
	/**
	 * 
	 */
	ormmaview.request = function(URI, display) {

	};
	/**
	 * 
	 */
	ormmaview.removeAsset = function(alias) {
	};

	ormmaview.stringify = function(args) {
		// console.log("inside ormmaview.stringify from ormma_bridge.js:");
		if (typeof JSON === "undefined") {
			var s = "";
			var len = args.length;
			var i;
			if (typeof len == "undefined") {
				return ormmaview.stringifyArg(args);
			}
			for (i = 0; i < args.length; i++) {
				if (i > 0) {
					s = s + ",";
				}
				s = s + ormmaview.stringifyArg(args[i]);
			}
			s = s + "]";
			return s;
		} else {
			return JSON.stringify(args);
		}
	};

	ormmaview.stringifyArg = function(arg) {
		var s, type, start, name, nameType, a;
		type = typeof arg;
		s = "";
		if ((type === "number") || (type === "boolean")) {
			s = s + args;
		} else if (arg instanceof Array) {
			s = s + "[" + arg + "]";
		} else if (arg instanceof Object) {
			start = true;
			s = s + '{';
			for (name in arg) {
				if (arg[name] !== null) {
					if (!start) {
						s = s + ',';
					}
					s = s + '"' + name + '":';
					nameType = typeof arg[name];
					if ((nameType === "number") || (nameType === "boolean")) {
						s = s + arg[name];
					} else if ((typeof arg[name]) === 'function') {
						// don't copy the functions
						s = s + '""';
					} else if (arg[name] instanceof Object) {
						s = s + this.stringify(args[i][name]);
					} else {
						s = s + '"' + arg[name] + '"';
					}
					start = false;
				}
			}
			s = s + '}';
		} else {
			a = arg.replace(/\\/g, '\\\\');
			a = a.replace(/"/g, '\\"');
			s = s + '"' + a + '"';
		}
		ormmaview.showAlert("json:" + s);
		return s;
	};

	var recordClicks = function(signature) {

		if (typeof (vservClickNotifyURL) != 'undefined'
				&& vservClickNotifyURL != null
				&& vservClickNotifyURL instanceof Array) {

			for ( var i in vservClickNotifyURL) {
				VservAdControllerInterface.notify(vservClickNotifyURL[i],
						signature);
			}
			MediationAdapterInterface.sendClickNotification();
		} else if (typeof (vservClickNotifyURL) != 'undefined'
				&& vservClickNotifyURL != null) {
			VservAdControllerInterface.notify(vservClickNotifyURL, signature);
			MediationAdapterInterface.sendClickNotification();
		}

	};

	var setPostActionNotifyUrl = function() {
		if (typeof (vservPostActionNotifyURL) != 'undefined'
				&& typeof (vservPostActionNotifyURL != null)
				&& typeof (vservPostActionNotifyURL) === 'string') {

			ORMMAUtilityControllerBridge
					.setPostActionNotifyUrl(vservPostActionNotifyURL);
		}
	};

	var clearPostActionNotifyUrl = function() {

		ORMMAUtilityControllerBridge.clearPostActionNotifyUrl();

	};

	window.onload = function() {
		var anchorElements = document.getElementsByTagName('a');
		for ( var i = 0; i < anchorElements.length; i++) {
			var attrHref = anchorElements[i].getAttribute('href');
			var attrOnClick = anchorElements[i].getAttribute('onclick');
			// console.log("attrHref::"+attrHref+" attrOnClick:"+attrOnClick);
			if (attrOnClick == undefined || attrOnClick == "") {
				anchorElements[i].onclick = function() {
					// record click for anchorElements[i].href
					// console.log(attrHref);
					if (attrHref != undefined && attrHref != null
							&& attrHref != "" && attrHref.substr(0, 1) != "#") {
						// console.log("record click:");
						recordClicks("open(" + attrHref + ")");
					}
					return true;
				}
			}

		}
	}

	vserv.sns = function(invokeUrl, exitUrl, shareUrl, successMessage) {
		console.log("*inside  vserv.sns from ormma_bridge.js:");
		try {

			ORMMADisplayControllerBridge.vservsns(invokeUrl, exitUrl, shareUrl,
					successMessage);

		} catch (e) {
			console.log("inside  vserv.sns from ormma_bridge.js e:" + e);
			// ormmaview.showAlert( "vserv.sns: " + e );
		}
	};

	vserv.makeCall = function(phoneNumber) {
		console.log("inside vserv.makeCall from ormma_bridge.js:");
		try {

			ORMMAUtilityControllerBridge.vservMakeCall(phoneNumber);
			recordClicks("vserv.makeCall(" + phoneNumber + ")");

		} catch (e) {
			ormmaview.showAlert("makeCall: " + e);
		}
	};

	vserv.sendSMS = function(recipient, body) {
		console.log("inside vserv.sendSMS from ormma_bridge.js:");
		try {

			ORMMAUtilityControllerBridge.vservSendSMS(recipient, body);
			recordClicks("vserv.sendSMS(" + recipient + "," + body + ")");

		} catch (e) {
			ormmaview.showAlert("sendSMS: " + e);
		}
	};

	vserv.addToContact = function(mobileNumber, firstName, lastName, email) {
		console.log("inside vserv.addToContact from ormma_bridge.js:");
		try {
			ORMMAUtilityControllerBridge.vservAddToContact(mobileNumber,
					firstName, lastName, email);
			recordClicks("vserv.addToContact(" + mobileNumber + "," + firstName
					+ "," + lastName + "," + email + ")");

		} catch (e) {
			ormmaview.showAlert("addToContact: " + e);
		}
	};

	vserv.open = function(url) {
		console
				.log("inside vserv.openUrlInDefaultBrowser from ormma_bridge.js:");
		try {

			ORMMAUtilityControllerBridge.vservOpenUrlInDefaultBrowser(url);

		} catch (e) {
			ormmaview.showAlert("openUrlInDefaultBrowser: " + e);
		}
	};

	vserv.createEvent = function(date, endDate, eventTitle, eventDetails,
			minute, reminder, until) {
		console.log("inside vserv.addToCalender from ormma_bridge.js:");
		try {
			ORMMAUtilityControllerBridge.vservAddToCalender(date, endDate,
					eventTitle, eventDetails, minute, reminder, until);
			recordClicks("vserv.createEvent(" + date + "," + endDate + ","
					+ eventTitle + "," + eventDetails + "," + minute + ","
					+ reminder + "," + until + ")");
		} catch (e) {
			ormmaview.showAlert("createEvent(" + date + "," + endDate + ","
					+ eventTitle + "," + eventDetails + "," + minute + ","
					+ reminder + "," + until + ")");
		}
	};

	vserv.recommendFriend = function(message, link) {

		console.log("inside vserv.recommendFriend from ormma_bridge.js:");
		try {

			ORMMAUtilityControllerBridge.vservRecommendFriend(message, link);
			recordClicks("vserv.recommendFriend(" + message + "," + link + ")");

		} catch (e) {
			ormmaview
					.showAlert("recommendFriend(" + message + "," + link + ")");
		}

	}

	vserv.download = function(url) {

		console.log("inside vserv.download from ormma_bridge.js:");

		try {

			if (typeof (vservPostActionNotifyURL) != 'undefined'
					&& typeof (vservPostActionNotifyURL != null)
					&& typeof (vservPostActionNotifyURL) === 'string') {

				ORMMAUtilityControllerBridge.vservDownload(url,
						vservPostActionNotifyURL);
			} else {
				ORMMAUtilityControllerBridge.vservDownload(url, null);
			}

			recordClicks("vserv.download(" + url + ")");

		} catch (e) {
			ormmaview.showAlert("download(" + url + ")");
		}

	}

	vserv.playVideo = function(URL, properties) {
		console.log("inside vserv.playVideo from ormma_bridge.js:");

		var audioMuted = false, autoPlay = false, controls = false, loop = false, position = [
				-1, -1, -1, -1 ], startStyle = 'normal', stopStyle = 'normal';
		if (properties != null) {

			if ((typeof properties.audio != "undefined")
					&& (properties.audio != null)) {
				audioMuted = properties.audio;
			}

			if ((typeof properties.autoplay != "undefined")
					&& (properties.autoplay != null)) {
				autoPlay = properties.autoplay;
			}

			if ((typeof properties.controls != "undefined")
					&& (properties.controls != null)) {
				controls = properties.controls;
			}

			if ((typeof properties.loop != "undefined")
					&& (properties.loop != null)) {
				loop = properties.loop;
			}

			if ((typeof properties.position != "undefined")
					&& (properties.position != null)) {
				position[0] = properties.position.top;
				position[1] = properties.position.left;
				position[2] = properties.width;
				position[3] = properties.height;
			}

			if ((typeof properties.startStyle != "undefined")
					&& (properties.startStyle != null)) {
				startStyle = properties.startStyle;
			}

			if ((typeof properties.stopStyle != "undefined")
					&& (properties.stopStyle != null)) {
				stopStyle = properties.stopStyle;
			}

			if (loop) {
				stopStyle = 'normal';
				controls = true;
			}

			if (!autoPlay)
				controls = true;

			if (!controls) {
				stopStyle = 'exit';
			}

			if (position[0] == -1 || position[1] == -1) {
				startStyle = "fullscreen";
			}
		}

		try {
			clearPostActionNotifyUrl();
			setPostActionNotifyUrl();
			ORMMADisplayControllerBridge.playVideo(URL, audioMuted, autoPlay,
					controls, loop, position, startStyle, stopStyle);
			recordClicks("vserv.playVideo(" + URL + "," + properties + ")");

		} catch (e) {
			ormmaview.showAlert("playVideo: " + e);
		}

	};

})();
