type AlexaDisplayCategory = // based on https://developer.amazon.com/de-DE/docs/alexa/device-apis/alexa-discovery.html#display-categories
    'ACTIVITY_TRIGGER'  // A combination of devices set to a specific state. Use activity triggers for scenes when the state changes must occur in a specific order. For example, for a scene named "watch Netflix" you might power on the TV first, and then set the input to HDMI1.
    | 'CAMERA'         // A media device with video or photo functionality.
    | 'COMPUTER'       // A non-mobile computer, such as a desktop computer.
    | 'CONTACT_SENSOR' // An endpoint that detects and reports changes in contact between two surfaces.
    | 'DOOR'           // A door.
    | 'DOORBELL'       // A doorbell.
    | 'EXTERIOR_BLIND' // A window covering on the outside of a structure.
    | 'FAN'            // A fan.
    | 'GAME_CONSOLE'   // A game console, such as Microsoft Xbox or Nintendo Switch
    | 'GARAGE_DOOR'    // A garage door. Garage doors must implement the ModeController interface to open and close the door.
    | 'INTERIOR_BLIND' // A window covering on the inside of a structure.
    | 'LAPTOP'         // A laptop or other mobile computer.
    | 'LIGHT'          // A light source or fixture.
    | 'MICROWAVE'      // A microwave oven.
    | 'MOBILE_PHONE'   // A mobile phone.
    | 'MOTION_SENSOR'  // An endpoint that detects and reports movement in an area.
    | 'MUSIC_SYSTEM'   // A network-connected music system.
    | 'NETWORK_HARDWARE'   // A network router.
    | 'OTHER'          // An endpoint that doesn't belong to one of the other categories.
    | 'OVEN'           // An oven cooking appliance.
    | 'PHONE'          // A non-mobile phone, such as landline or an IP phone.
    | 'SCENE_TRIGGER'  // A combination of devices set to a specific state. Use scene triggers for scenes when the order of the state change is not important. For example, for a scene named "bedtime" you might turn off the lights and lower the thermostat, in any order.
    | 'SCREEN'         // A projector screen.
    | 'SECURITY_PANEL' // A security panel.
    | 'SMARTLOCK'      // An endpoint that locks.
    | 'SMARTPLUG'      // A module that is plugged into an existing electrical outlet, and then has a device plugged into it. For example, a user can plug a smart plug into an outlet, and then plug a lamp into the smart plug. A smart plug can control a variety of devices.
    | 'SPEAKER'        // A speaker or speaker system.
    | 'STREAMING_DEVICE'   // A streaming device such as Apple TV, Chromecast, or Roku.
    | 'SWITCH'         // A switch wired directly to the electrical system. A switch can control a variety of devices.
    | 'TABLET'         // A tablet computer.
    | 'TEMPERATURE_SENSOR' // An endpoint that reports temperature, but does not control it. The temperature data of the endpoint is not shown in the Alexa app. If your endpoint also controls temperature, use THERMOSTAT instead.
    | 'THERMOSTAT'     // An endpoint that controls temperature, stand-alone air conditioners, or heaters with direct temperature control. If your endpoint senses temperature but does not control it, use TEMPERATURE_SENSOR instead.
    | 'TV'             // A television.
    | 'WEARABLE'       // A network-connected wearable device, such as an Apple Watch, Fitbit, or Samsung Gear.
    ;

interface AlexaEndpoint {
    endpointId: string;         // unique ID of the endpoint
    manufacturerName: string;
    description: string;
    friendlyName: string;
    displayCategories: AlexaDisplayCategory[];
    additionalAttributes: any | undefined;
    capabilities: any[];
    connections: any[] | undefined;
    relationships: any | undefined;
    cookie: any | undefined;
}

/**
 * The Alexa.BrightnessController capability interface describes messages used to
 * control the brightness of an endpoint such as a light bulb. Use this interface
 * for devices that you know support brightness control instead of the more general
 * Alexa.PercentageController interface. If you want to control the power level of
 * an endpoint such as a dimmer switch, use the Alexa.PowerLevelController interface
 * instead.
 * https://developer.amazon.com/de-DE/docs/alexa/device-apis/alexa-brightnesscontroller.html
 */
interface AlexaBrightnessController {
    brightness: number;
}

/**
 * Implement the Alexa.PowerController capability interface in your Alexa skills
 * so that users can turn their smart home devices on and off.
 * https://developer.amazon.com/de-DE/docs/alexa/device-apis/alexa-powercontroller.html
 */
interface AlexaPowerController {
    powerState: 'ON' | 'OFF';
}

// Jalousinen?
// https://developer.amazon.com/de-DE/docs/alexa/device-apis/alexa-togglecontroller.html

// https://developer.amazon.com/de-DE/docs/alexa/device-apis/alexa-scenecontroller.html

// https://developer.amazon.com/de-DE/docs/alexa/device-apis/alexa-endpointhealth.html

// https://developer.amazon.com/de-DE/docs/alexa/device-apis/alexa-modecontroller.html

// Control a TV later:
// - https://developer.amazon.com/de-DE/docs/alexa/device-apis/alexa-inputcontroller.html
// - https://developer.amazon.com/de-DE/docs/alexa/device-apis/alexa-channelcontroller.html

