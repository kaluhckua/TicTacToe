/// <reference path="class.js" />
var signalR = (function () {

    var SignalR = Class.create({
        init: function (hub) {
            this.$.connection.hub;
        }
    });

    return {
        get: function (hub) {
            return new SignalR(hub);
        }

    }


}())