/*  Copyright (c) 2011 The ORMMA.org project authors. All Rights Reserved.
 *
 *  Use of this source code is governed by a BSD-style license
 *  that can be found in the LICENSE file in the root of the source
 *  tree. All contributing project authors may
 *  be found in the AUTHORS file in the root of the source tree.
 */

(function() {
	var ormma = window.ormma = {};
	var mraid = window.mraid = {};

	var STATES = ormma.STATES = {
		UNKNOWN : 'unknown',
		DEFAULT : 'default',
		RESIZED : 'resized',
		EXPANDED : 'expanded',
		HIDDEN : 'hidden'
	};
	mraid.STATES = {
		LOADING : 'loading',
		DEFAULT : 'default',
		EXPANDED : 'expanded',
		HIDDEN : 'hidden'
	};

	var EVENTS = ormma.EVENTS = {
		ASSETREADY : 'assetReady',
		ASSETREMOVED : 'assetRemoved',
		ASSETRETIRED : 'assetRetired',
		ERROR : 'error',
		INFO : 'info',
		HEADINGCHANGE : 'headingChange',
		KEYBOARDCHANGE : 'keyboardChange',
		LOCATIONCHANGE : 'locationChange',
		NETWORKCHANGE : 'networkChange',
		ORIENTATIONCHANGE : 'orientationChange',
		RESPONSE : 'response',
		SCREENCHANGE : 'screenChange',
		SHAKE : 'shake',
		SIZECHANGE : 'sizeChange',
		STATECHANGE : 'stateChange',
		TILTCHANGE : 'tiltChange'
	};
	mraid.EVENTS = {
		READY : 'ready',
		ERROR : 'error',
		INFO : 'info',
		STATECHANGE : 'stateChange',
		VIEWABLECHANGE : 'viewableChange'
	};

	var CONTROLS = ormma.CONTROLS = {
		BACK : 'back',
		FORWARD : 'forward',
		REFRESH : 'refresh',
		ALL : 'all'
	};

	var FEATURES = ormma.FEATURES = {
		LEVEL1 : 'level-1',
		LEVEL2 : 'level-2',
		LEVEL3 : 'level-3',
		SCREEN : 'screen',
		ORIENTATION : 'orientation',
		HEADING : 'heading',
		LOCATION : 'location',
		SHAKE : 'shake',
		TILT : 'tilt',
		NETWORK : 'network',
		SMS : 'sms',
		PHONE : 'phone',
		EMAIL : 'email',
		CALENDAR : 'calendar',
		CAMERA : 'camera',
		AUDIO : 'audio',
		VIDEO : 'video',
		MAP : 'map'
	};

	var NETWORK = ormma.NETWORK = {
		OFFLINE : 'offline',
		WIFI : 'wifi',
		CELL : 'cell',
		UNKNOWN : 'unknown'
	};

	// PRIVATE PROPERTIES (sdk controlled)
	// //////////////////////////////////////////////////////

	var state = STATES.UNKNOWN;

	var size = {
		width : 0,
		height : 0
	};

	var defaultPosition = {
		x : 0,
		y : 0,
		width : 0,
		height : 0
	};

	var maxSize = {
		width : 0,
		height : 0
	};

	var supports = {
		'level-1' : true,
		'level-2' : true,
		'level-3' : true,
		'screen' : true,
		'orientation' : true,
		'heading' : true,
		'location' : true,
		'shake' : true,
		'tilt' : true,
		'network' : true,
		'sms' : true,
		'phone' : true,
		'email' : true,
		'calendar' : true,
		'camera' : true,
		'audio' : true,
		'video' : true,
		'map' : true
	};

	var heading = -1;

	var keyboardState = false;

	var location = null;

	var network = NETWORK.UNKNOWN;

	var orientation = -1;

	var screenSize = null;

	var shakeProperties = null;

	var tilt = null;

	var assets = {};

	var cacheRemaining = -1;

	// PRIVATE PROPERTIES (internal)
	// //////////////////////////////////////////////////////

	var intervalID = null;
	var readyTimeout = 10000;
	var readyDuration = 0;

	var dimensionValidators = {
		x : function(value) {
			return !isNaN(value) && value >= 0;
		},
		y : function(value) {
			return !isNaN(value) && value >= 0;
		},
		width : function(value) {
			return !isNaN(value) && value >= 0 && value <= screenSize.width;
		},
		height : function(value) {
			return !isNaN(value) && value >= 0 && value <= screenSize.height;
		}
	};

	var expandPropertyValidators = {
		useBackground : function(value) {
			return (value === true || value === false);
		},
		backgroundColor : function(value) {
			return (typeof value == 'string' && value.substr(0, 1) == '#' && !isNaN(parseInt(
					value.substr(1), 16)));
		},
		backgroundOpacity : function(value) {
			return !isNaN(value) && value >= 0 && value <= 1;
		},
		lockOrientation : function(value) {
			return (value === true || value === false);
		}
	};

	var shakePropertyValidators = {
		intensity : function(value) {
			return !isNaN(value);
		},
		interval : function(value) {
			return !isNaN(value);
		}
	};

	var changeHandlers = {

		state : function(val) {

			if (state == STATES.UNKNOWN) {
				// console.log("inside changeHandlers state == STATES.UNKNOWN
				// from ormma.js:"+val);
				intervalID = window.setInterval(window.ormma.signalReady, 20);
				broadcastEvent(EVENTS.INFO,
						'controller initialized, attempting callback');
			}
			broadcastEvent(EVENTS.INFO, 'setting state to ' + stringify(val));
			state = val;
			broadcastEvent(EVENTS.STATECHANGE, state);
		},
		size : function(val) {
			// console.log("inside changeHandlers size from
			// ormma.js:"+val+":"+stringify(val));
			broadcastEvent(EVENTS.INFO, 'setting size to ' + stringify(val));
			size = val;
			broadcastEvent(EVENTS.SIZECHANGE, size.width, size.height);
		},
		defaultPosition : function(val) {
			// console.log("inside changeHandlers defaultPosition from
			// ormma.js:"+val+":"+stringify(val));
			broadcastEvent(EVENTS.INFO, 'setting default position to '
					+ stringify(val));
			defaultPosition = val;
		},
		maxSize : function(val) {
			// console.log("inside changeHandlers maxSize from
			// ormma.js:"+val+":"+stringify(val));
			broadcastEvent(EVENTS.INFO, 'setting maxSize to ' + stringify(val));
			maxSize = val;
		},
		expandProperties : function(val) {
			// console.log("inside changeHandlers expandProperties from
			// ormma.js:"+val+":"+stringify(val));
			broadcastEvent(EVENTS.INFO, 'merging expandProperties with '
					+ stringify(val));
			for ( var i in val) {
				expandProperties[i] = val[i];
			}
		},
		supports : function(val) {
			// console.log("inside changeHandlers supports from
			// ormma.js:"+val+":"+stringify(val));
			broadcastEvent(EVENTS.INFO, 'setting supports to ' + stringify(val));
			supports = {};
			for ( var key in FEATURES) {
				supports[FEATURES[key]] = contains(FEATURES[key], val);
			}
		},
		heading : function(val) {
			// console.log("inside changeHandlers heading from
			// ormma.js:"+val+":"+stringify(val));
			broadcastEvent(EVENTS.INFO, 'setting heading to ' + stringify(val));
			heading = val;
			broadcastEvent(EVENTS.HEADINGCHANGE, heading);
		},
		keyboardState : function(val) {
			// console.log("inside changeHandlers keyboardState from
			// ormma.js:"+val+":"+stringify(val));
			broadcastEvent(EVENTS.INFO, 'setting keyboardState to '
					+ stringify(val));
			keyboardState = val;
			broadcastEvent(EVENTS.KEYBOARDCHANGE, keyboardState);
		},
		location : function(val) {
			// console.log("inside changeHandlers location from
			// ormma.js:"+val+":"+stringify(val));
			broadcastEvent(EVENTS.INFO, 'setting location to ' + stringify(val));
			location = val;
			broadcastEvent(EVENTS.LOCATIONCHANGE, location.lat, location.lon,
					location.acc);
		},
		network : function(val) {
			// console.log("inside changeHandlers network from
			// ormma.js:"+val+":"+stringify(val));
			broadcastEvent(EVENTS.INFO, 'setting network to ' + stringify(val));
			network = val;
			broadcastEvent(EVENTS.NETWORKCHANGE,
					(network != NETWORK.OFFLINE && network != NETWORK.UNKNOWN),
					network);
		},
		orientation : function(val) {
			// console.log("inside changeHandlers orientation from
			// ormma.js:"+val+":"+stringify(val));
			broadcastEvent(EVENTS.INFO, 'setting orientation to '
					+ stringify(val));
			orientation = val;
			broadcastEvent(EVENTS.ORIENTATIONCHANGE, orientation);
		},
		screenSize : function(val) {
			// console.log("inside changeHandlers screenSize from
			// ormma.js:"+val+":"+stringify(val));
			broadcastEvent(EVENTS.INFO, 'setting screenSize to '
					+ stringify(val));
			screenSize = val;
			broadcastEvent(EVENTS.SCREENCHANGE, screenSize.width,
					screenSize.height);
		},
		shakeProperties : function(val) {
			// console.log("inside changeHandlers shakeProperties from
			// ormma.js:"+val+":"+stringify(val));
			broadcastEvent(EVENTS.INFO, 'setting shakeProperties to '
					+ stringify(val));
			shakeProperties = val;
		},
		tilt : function(val) {
			// console.log("inside changeHandlers tilt from
			// ormma.js:"+val+":"+stringify(val));
			broadcastEvent(EVENTS.INFO, 'setting tilt to ' + stringify(val));
			tilt = val;
			broadcastEvent(EVENTS.TILTCHANGE, tilt.x, tilt.y, tilt.z);
		},
		cacheRemaining : function(val) {
			// console.log("inside changeHandlers cacheRemaining from
			// ormma.js:"+val+":"+stringify(val));
			broadcastEvent(EVENTS.INFO, 'setting cacheRemaining to '
					+ stringify(val));
			cacheRemaining = val;
		}
	};

	var listeners = {};

	var EventListeners = function(event) {
		console.log("^^^^^^^^^^^^inside EventListeners from ormma.js:");
		this.event = event;
		this.count = 0;
		var listeners = {};

		this.add = function(func) {
			var id = String(func);
			if (!listeners[id]) {
				console.log("inside EventListeners activate from ormma.js:");
				listeners[id] = func;
				this.count++;
				if (this.count == 1)
					ormmaview.activate(event);
			}
		};
		this.remove = function(func) {
			var id = String(func);
			if (listeners[id]) {
				console.log("inside EventListeners deactivate from ormma.js:");
				listeners[id] = null;
				delete listeners[id];
				this.count--;
				if (this.count == 0)
					ormmaview.deactivate(event);
				return true;
			} else {
				return false;
			}
		};
		this.removeAll = function() {
			for ( var id in listeners)
				this.remove(listeners[id]);
		};
		this.broadcast = function(args) {
			for ( var id in listeners)
				listeners[id].apply({}, args);
		};
		this.toString = function() {
			var out = [ event, ':' ];
			for ( var id in listeners)
				out.push('|', id, '|');
			return out.join('');
		};
	};

	// PRIVATE METHODS
	// ////////////////////////////////////////////////////////////

	ormmaview.addEventListener('change', function(properties) {
		// console.log("inside ormmaview.addEventListener change from
		// ormma.js:properties::"+properties);
		for ( var property in properties) {
			// console.log("##inside ormmaview.addEventListener change from
			// ormma.js:property:"+property);
			var handler = changeHandlers[property];
			// console.log("%%handler:"+handler);
			// console.log("%%properties[property]:"+properties[property]);
			handler(properties[property]);
		}
	});

	ormmaview.addEventListener('shake', function() {
		console.log("inside ormmaview.addEventListener shake from ormma.js:");
		broadcastEvent(EVENTS.SHAKE);
	});

	ormmaview.addEventListener('error', function(message, action) {
		console.log("inside ormmaview.addEventListener error from ormma.js:");
		broadcastEvent(EVENTS.ERROR, message, action);
	});

	ormmaview
			.addEventListener(
					'response',
					function(uri, response) {
						console
								.log("inside ormmaview.addEventListener response from ormma.js:");
						broadcastEvent(EVENTS.RESPONSE, uri, response);
					});

	ormmaview
			.addEventListener(
					'assetReady',
					function(alias, URL) {
						console
								.log("inside ormmaview.addEventListener assetReady from ormma.js:");
						assets[alias] = URL;
						broadcastEvent(EVENTS.ASSETREADY, alias);
					});

	ormmaview
			.addEventListener(
					'assetRemoved',
					function(alias) {
						console
								.log("inside ormmaview.addEventListener assetRemoved from ormma.js:");
						assets[alias] = null;
						delete assets[alias];
						broadcastEvent(EVENTS.ASSETREMOVED, alias);
					});

	ormmaview
			.addEventListener(
					'assetRetired',
					function(alias) {
						console
								.log("inside ormmaview.addEventListener assetRetired from ormma.js:");
						assets[alias] = null;
						delete assets[alias];
						broadcastEvent(EVENTS.ASSETRETIRED, alias);
					});

	var clone = function(obj) {
		console.log("inside clone from ormma.js:");
		var f = function() {
		};
		f.prototype = obj;
		return new f();
	};

	var stringify = function(obj) {
		// console.log("inside stringify from ormma.js:");
		if (typeof obj == 'object') {
			if (obj.push) {
				var out = [];
				for ( var p in obj) {
					out.push(obj[p]);
				}
				return '[' + out.join(',') + ']';
			} else {
				var out = [];
				for ( var p in obj) {
					out.push('\'' + p + '\':' + obj[p]);
				}
				return '{' + out.join(',') + '}';
			}
		} else {
			return String(obj);
		}
	};

	var valid = function(obj, validators, action, full) {
		console.log("inside valid from ormma.js:");
		if (full) {
			if (obj === undefined) {
				broadcastEvent(EVENTS.ERROR, 'Required object missing.', action);
				return false;
			} else {
				for ( var i in validators) {
					if (obj[i] === undefined) {
						broadcastEvent(EVENTS.ERROR,
								'Object missing required property ' + i, action);
						return false;
					}
				}
			}
		}
		for ( var i in obj) {
			if (!validators[i]) {
				broadcastEvent(EVENTS.ERROR, 'Invalid property specified - '
						+ i + '.', action);
				return false;
			} else if (!validators[i](obj[i])) {
				broadcastEvent(EVENTS.ERROR, 'Value of property ' + i + '<'
						+ obj[i] + '>' + ' is not valid type.', action);
				return false;
			}
		}
		return true;
	};

	var contains = function(value, array) {
		// console.log("inside contains from ormma.js:");
		for ( var i in array)
			if (array[i] == value)
				return true;
		return false;
	};

	var broadcastEvent = function() {
		// console.log("inside broadcastEvent from ormma.js:");
		var args = new Array(arguments.length);
		for ( var i = 0; i < arguments.length; i++) {
			// console.log("###inside broadcastEvent from
			// ormma.js:arguments[i]:"+arguments[i]);
			args[i] = arguments[i];
		}

		var event = args.shift();
		try {
			// console.log("^^^^event:"+event+" args:"+args);
			if (listeners[event])
				listeners[event].broadcast(args);
		} catch (e) {
		}
	}

	var trim = function(s) {
		console.log("inside trim from ormma.js:");
		var l = 0;
		var r = s.length - 1;
		while (l < s.length && s[l] == ' ') {
			l++;
		}
		while (r > l && s[r] == ' ') {
			r -= 1;
		}
		return s.substring(l, r + 1);
	}

	// LEVEL 1
	// ////////////////////////////////////////////////////////////////////

	ormma.signalReady = function() {
		// console.log("inside ormma.signalReady from ormma.js000:");
		if (typeof ORMMAReady == 'function') {
			// console.log("inside ormma.signalReady from ormma.js111:");
			window.clearInterval(intervalID);
			ORMMAReady();
			broadcastEvent(EVENTS.INFO, 'callback invoked');
			broadcastEvent(mraid.EVENTS.READY, 'mraid ready event triggered');
		} else {
			readyDuration += 20;
			if (readyDuration >= readyTimeout) {
				window.clearInterval(intervalID);
				broadcastEvent(EVENTS.ERROR, 'Callback not found (timeout of '
						+ readyTimeout + 'ms occurred)!');
			}
		}
	};

	ormma.addEventListener = function(event, listener) {
		console.log("inside ormma.addEventListener from ormma.js:");
		if (!event || !listener) {
			broadcastEvent(EVENTS.ERROR,
					'Both event and listener are required.', 'addEventListener');
		} else if (!contains(event, EVENTS)) {
			broadcastEvent(EVENTS.ERROR, 'Unknown event: ' + event,
					'addEventListener');
		} else {
			if (!listeners[event])
				listeners[event] = new EventListeners(event);
			listeners[event].add(listener);
		}
	};

	ormma.close = function() {
		console.log("inside ormma.close from ormma.js:");
		ormmaview.close();
	};

	ormma.expand = function(dimensions, URL) {
		console.log("inside ormma.expand from ormma.js:");
		broadcastEvent(EVENTS.INFO, 'expanding to ' + stringify(dimensions));
		if (valid(dimensions, dimensionValidators, 'expand', true)) {
			ormmaview.expand(dimensions, URL);
		}
	};

	ormma.getDefaultPosition = function() {
		console.log("inside ormma.getDefaultPosition from ormma.js:");
		return clone(defaultPosition);
	};

	ormma.getExpandProperties = function() {
		console.log("inside ormma.getExpandProperties from ormma.js:");
		return clone(ormmaview.getExpandProperties());
	};

	ormma.getMaxSize = function() {
		console.log("inside ormma.getMaxSize from ormma.js:");
		return clone(maxSize);
	};

	ormma.getSize = function() {
		console.log("inside ormma.getSize from ormma.js:");
		return clone(size);
	};

	ormma.getState = function() {
		console.log("inside ormma.getState from ormma.js:");
		return state;
	};

	ormma.hide = function() {
		console.log("inside ormma.hide from ormma.js:");
		if (state == STATES.HIDDEN) {
			broadcastEvent(EVENTS.ERROR, 'Ad is currently hidden.', 'hide');
		} else {
			ormmaview.hide();
		}
	};

	ormma.open = function(URL, controls) {
		console.log("inside ormma.open from ormma.js:");
		if (!URL) {
			broadcastEvent(EVENTS.ERROR, 'URL is required.', 'open');
		} else {
			ormmaview.open(URL, controls);
		}
	};

	ormma.openMap = function(POI, fullscreen) {
		console.log("inside ormma.openMap from ormma.js:");
		if (!POI) {
			broadcastEvent(EVENTS.ERROR, 'POI is required.', 'openMap');
		} else {
			ormmaview.openMap(POI, fullscreen);
		}
	};

	ormma.removeEventListener = function(event, listener) {
		console.log("inside ormma.removeEventListener from ormma.js:");
		if (!event) {
			broadcastEvent(EVENTS.ERROR, 'Must specify an event.',
					'removeEventListener');
		} else {
			if (listener
					&& (!listeners[event] || !listeners[event].remove(listener))) {
				broadcastEvent(EVENTS.ERROR,
						'Listener not currently registered for event',
						'removeEventListener');
				return;
			} else if (listeners[event]) {
				listeners[event].removeAll();
			}

			if (listeners[event] && listeners[event].count == 0) {
				listeners[event] = null;
				delete listeners[event];
			}
		}
	};

	ormma.resize = function(width, height) {
		console.log("inside ormma.resize from ormma.js:");
		if (width == null || height == null || isNaN(width) || isNaN(height)
				|| width < 0 || height < 0) {
			broadcastEvent(
					EVENTS.ERROR,
					'Requested size must be numeric values between 0 and maxSize.',
					'resize');
		} else if (width > maxSize.width || height > maxSize.height) {
			broadcastEvent(EVENTS.ERROR, 'Request (' + width + ' x ' + height
					+ ') exceeds maximum allowable size of (' + maxSize.width
					+ ' x ' + maxSize.height + ')', 'resize');
		} else if (width == size.width && height == size.height) {
			broadcastEvent(EVENTS.ERROR, 'Requested size equals current size.',
					'resize');
		} else {
			ormmaview.resize(width, height);
		}
	};

	ormma.setExpandProperties = function(properties) {
		console.log("inside ormma.setExpandProperties from ormma.js:");
		if (valid(properties, expandPropertyValidators, 'setExpandProperties')) {
			ormmaview.setExpandProperties(properties);
		}
	};

	ormma.show = function() {
		console.log("inside ormma.show from ormma.js:");
		if (state != STATES.HIDDEN) {
			broadcastEvent(EVENTS.ERROR, 'Ad is currently visible.', 'show');
		} else {
			ormmaview.show();
		}
	};

	ormma.playAudio = function(URL, properties) {
		console.log("inside ormma.playAudio from ormma.js:");
		if (!supports[FEATURES.AUDIO]) {
			broadcastEvent(EVENTS.ERROR,
					'Method not supported by this client.', 'playAudio');
		} else if (!URL || typeof URL != 'string') {
			broadcastEvent(EVENTS.ERROR, 'Request must specify a URL',
					'playAudio');
		} else {
			ormmaview.playAudio(URL, properties);
		}
	};

	ormma.playVideo = function(URL, properties) {
		console.log("inside ormma.playVideo from ormma.js:");
		if (!supports[FEATURES.VIDEO]) {
			broadcastEvent(EVENTS.ERROR,
					'Method not supported by this client.', 'playVideo');
		} else if (!URL || typeof URL != 'string') {
			broadcastEvent(EVENTS.ERROR, 'Request must specify a URL',
					'playVideo');
		} else {
			console.log("Calling ormma.playVideo in ormma_bridge");
			ormmaview.playVideo(URL, properties);
		}
	};

	// LEVEL 2
	// ////////////////////////////////////////////////////////////////////

	ormma.createEvent = function(date, title, body) {
		console.log("inside ormma.createEvent from ormma.js:");
		if (!supports[FEATURES.CALENDAR]) {
			console.log("inside ormma.createEvent from ormma.js:");
			broadcastEvent(EVENTS.ERROR,
					'Method not supported by this client.', 'createEvent');
		} else if (!date || typeof date != 'string') {
			console.log("Valid date required");
			broadcastEvent(EVENTS.ERROR, 'Valid date required.', 'createEvent');
		} else if (!title || typeof title != 'string'
				|| trim(title).length == 0) {
			console.log("Valid title required.");
			broadcastEvent(EVENTS.ERROR, 'Valid title required.', 'createEvent');
		} else if (!body || typeof body != 'string' || trim(body).length == 0) {
			console.log("Valid body required.");
			broadcastEvent(EVENTS.ERROR, 'Valid body required.', 'createEvent');
		} else {
			console.log("Calling ormmaview.createEvent");
			ormmaview.createEvent(date, title, body);
		}
	};

	ormma.getHeading = function() {
		console.log("inside ormma.getHeading from ormma.js:");
		if (!supports[FEATURES.HEADING]) {
			broadcastEvent(EVENTS.ERROR,
					'Method not supported by this client.', 'getHeading');
		}
		return heading;
	};

	ormma.getKeyboardState = function() {
		console.log("inside ormma.getKeyboardState from ormma.js:");
		if (!supports[FEATURES.LEVEL2]) {
			broadcastEvent(EVENTS.ERROR,
					'Method not supported by this client.', 'getKeyboardState');
		}
		return keyboardState;
	}

	ormma.getLocation = function() {
		console.log("inside ormma.getLocation from ormma.js:");
		if (!supports[FEATURES.LOCATION]) {
			broadcastEvent(EVENTS.ERROR,
					'Method not supported by this client.', 'getLocation');
		}
		return (null == location) ? null : clone(location);
	};

	ormma.getNetwork = function() {
		console.log("inside ormma.getNetwork from ormma.js:");
		if (!supports[FEATURES.NETWORK]) {
			broadcastEvent(EVENTS.ERROR,
					'Method not supported by this client.', 'getNetwork');
		}
		return network;
	};

	ormma.getOrientation = function() {
		console.log("inside ormma.getOrientation from ormma.js:");
		if (!supports[FEATURES.ORIENTATION]) {
			broadcastEvent(EVENTS.ERROR,
					'Method not supported by this client.', 'getOrientation');
		}
		return orientation;
	};

	ormma.getScreenSize = function() {
		console.log("inside ormma.getScreenSize from ormma.js:");
		if (!supports[FEATURES.SCREEN]) {
			broadcastEvent(EVENTS.ERROR,
					'Method not supported by this client.', 'getScreenSize');
		} else {
			return (null == screenSize) ? null : clone(screenSize);
		}
	};

	ormma.getShakeProperties = function() {
		console.log("inside ormma.getShakeProperties from ormma.js:");
		if (!supports[FEATURES.SHAKE]) {
			broadcastEvent(EVENTS.ERROR,
					'Method not supported by this client.',
					'getShakeProperties');
		} else {
			return (null == shakeProperties) ? null : clone(shakeProperties);
		}
	};

	ormma.getTilt = function() {
		console.log("inside ormma.getTilt from ormma.js:");
		if (!supports[FEATURES.TILT]) {
			broadcastEvent(EVENTS.ERROR,
					'Method not supported by this client.', 'getTilt');
		} else {
			return (null == tilt) ? null : clone(tilt);
		}
	};

	ormma.makeCall = function(number) {
		console.log("inside ormma.makeCall from ormma.js:");
		if (!supports[FEATURES.PHONE]) {
			broadcastEvent(EVENTS.ERROR,
					'Method not supported by this client.', 'makeCall');
		} else if (!number || typeof number != 'string'
				|| trim(number).length == 0) {
			broadcastEvent(EVENTS.ERROR,
					'Request must provide a number to call.', 'makeCall');
		} else {
			ormmaview.makeCall(number);
		}
	};

	ormma.sendMail = function(recipient, subject, body) {
		console.log("inside ormma.sendMail from ormma.js:");
		if (!supports[FEATURES.EMAIL]) {
			broadcastEvent(EVENTS.ERROR,
					'Method not supported by this client.', 'sendMail');
		} else if (!recipient || typeof recipient != 'string'
				|| trim(recipient).length == 0) {
			broadcastEvent(EVENTS.ERROR, 'Request must specify a recipient.',
					'sendMail');
		} else {
			ormmaview.sendMail(recipient, subject, body);
		}
	};

	ormma.sendSMS = function(recipient, body) {
		console.log("inside ormma.sendSMS from ormma.js:");
		if (!supports[FEATURES.SMS]) {
			broadcastEvent(EVENTS.ERROR,
					'Method not supported by this client.', 'sendSMS');
		} else if (!recipient || typeof recipient != 'string'
				|| trim(recipient).length == 0) {
			broadcastEvent(EVENTS.ERROR, 'Request must specify a recipient.',
					'sendSMS');
		} else {
			ormmaview.sendSMS(recipient, body);
		}
	};

	ormma.setShakeProperties = function(properties) {
		console.log("inside ormma.setShakeProperties from ormma.js:");
		if (!supports[FEATURES.SHAKE]) {
			broadcastEvent(EVENTS.ERROR,
					'Method not supported by this client.',
					'setShakeProperties');
		} else if (valid(properties, shakePropertyValidators,
				'setShakeProperties')) {
			ormmaview.setShakeProperties(properties);
		}
	};

	// ormma.storePicture = function(URL) {};

	ormma.supports = function(feature) {
		console.log("inside ormma.supports from ormma.js:");

		if (supports[feature]) {
			return true;
		} else {
			return false;
		}
	};

	// LEVEL 3
	// ////////////////////////////////////////////////////////////////////

	ormma.addAsset = function(URL, alias) {
		console.log("inside ormma.addAsset from ormma.js:");
		if (!URL || !alias || typeof URL != 'string'
				|| typeof alias != 'string') {
			broadcastEvent(EVENTS.ERROR, 'URL and alias are required.',
					'addAsset');
		} else if (supports[FEATURES.LEVEL3]) {
			ormmaview.addAsset(URL, alias);
		} else if (URL.indexOf('ormma://') == 0) {
			broadcastEvent(EVENTS.ERROR,
					'Native device assets not supported by this client.',
					'addAsset');
		} else {
			assets[alias] = URL;
			broadcastEvent(EVENTS.ASSETREADY, alias);
		}
	};

	ormma.addAssets = function(assets) {
		console.log("inside ormma.addAssets from ormma.js:");
		for ( var alias in assets) {
			ormma.addAsset(assets[alias], alias);
		}
	};

	ormma.getAssetURL = function(alias) {
		console.log("inside ormma.getAssetURL from ormma.js:");
		if (!assets[alias]) {
			broadcastEvent(EVENTS.ERROR, 'Alias unknown.', 'getAssetURL');
		}
		return assets[alias];
	};

	ormma.getCacheRemaining = function() {
		console.log("inside ormma.getCacheRemaining from ormma.js:");
		if (!supports[FEATURES.LEVEL3]) {
			broadcastEvent(EVENTS.ERROR,
					'Method not supported by this client.', 'getCacheRemaining');
		}
		return cacheRemaining;
	};

	ormma.request = function(uri, display) {
		console.log("inside ormma.request from ormma.js:");
		if (!supports[FEATURES.LEVEL3]) {
			broadcastEvent(EVENTS.ERROR,
					'Method not supported by this client.', 'request');
		} else if (!uri || typeof uri != 'string') {
			broadcastEvent(EVENTS.ERROR, 'URI is required.', 'request');
		} else {
			ormmaview.request(uri, display);
		}
	};

	ormma.removeAllAssets = function() {
		console.log("inside ormma.removeAllAssets from ormma.js:");
		for ( var alias in assets) {
			ormma.removeAsset(alias);
		}
	};

	ormma.removeAsset = function(alias) {
		console.log("inside ormma.removeAsset from ormma.js:");
		if (!alias || typeof alias != 'string') {
			broadcastEvent(EVENTS.ERROR, 'Alias is required.', 'removeAsset');
		} else if (!assets[alias]) {
			broadcastEvent(EVENTS.ERROR, 'Alias unknown.', 'removeAsset');
		} else if (supports[FEATURES.LEVEL3]) {
			ormmaview.removeAsset(alias);
		} else {
			assets[alias] = null;
			delete assets[alias];
			broadcastEvent(EVENTS.ASSETREMOVED, alias);
		}
	};

	// MRAID ////////////////////
	mraid.readyTimeout = function() {
		window.clearInterval(intervalID);
		broadcastEvent(EVENTS.ERROR,
				'No MRAID ready listener found (timeout of ' + readyTimeout
						+ 'ms occurred)');
	};

	mraid.getVersion = function() {
		return ('1.0');
	};

	mraid.addEventListener = ormma.addEventListener;
	mraid.close = ormma.close;
	mraid.expand = ormma.expand;
	mraid.getExpandProperties = ormma.getExpandProperties;
	mraid.getState = ormma.getState;

	mraid.removeEventListener = ormma.removeEventListener;
	mraid.setExpandProperties = ormma.setExpandProperties;
	mraid.useCustomClose = ormma.useCustomClose;
	mraid.error = ormma.error;

	mraid.open = ormma.open;
	mraid.makeCall = ormma.makeCall;
	mraid.sendMail = ormma.sendMail;
	mraid.sendSMS = ormma.sendSMS;
	mraid.openMap = ormma.openMap;
	mraid.playAudio = ormma.playAudio;
	mraid.playVideo = ormma.playVideo;
	mraid.createEvent = ormma.createEvent;

})();