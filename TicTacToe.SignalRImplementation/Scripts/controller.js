/// <reference path="class.js" />
/// <reference path="persister.js" />
/// <reference path="jquery-2.0.3.js" />
/// <reference path="ui.js" />

var controllers = (function () {

    var updateTimer = null;

    var rootUrl = "http://localhost:40643/api/";
    var Controller = Class.create({
        init: function (selector) {
            this.persister = persisters.get(selector);
            this.AttachUIEventHandlers(selector);
        },
        loadUI: function (selector) {
            if (this.persister.isUserLoggedIn()) {
                var gameHtml = ui.gameUI(this.persister.nickname());
                $(selector).html(gameHtml);
                $("#games-list-container").tabs({
                    collapsible: true
                });
                this.persister.user.registerConnectionId();
                this.persister.game.open();
                this.persister.game.active();
                this.persister.game.created();
                this.persister.game.joined();
            }
            else {
                this.loadLoginFormUI(selector);
            }
        },
        loadLoginFormUI: function (selector) {
            var loginFormHtml = ui.loginForm()
            $(selector).html(loginFormHtml);
        },
        LoadRegisterFormUI: function (selector) {
            var registerFormHtml = ui.registerForm();
            $(selector).html(registerFormHtml);
        },
        LoadGameUI: function (selector) {
            var gameHtml = ui.gameUI();
            $(selector).html(gameHtml);
        },
        loadJoinFormUI: function (selector, ispassword) {
            var joinForm = ui.joinForm(ispassword);
            $(selector).append(joinForm);
        },
        AttachUIEventHandlers: function (selector) {
            var wrapper = $(selector);
            var self = this;
            wrapper.on("click", "#btn-show-login", function () {
                self.loadLoginFormUI(selector);
            });
            wrapper.on("click", "#btn-show-register", function () {
                self.LoadRegisterFormUI(selector);
            });
            wrapper.on("click", "#btn-login", function () {
                var user = {
                    username: wrapper.find("#tb-login-username").val(),
                    password: wrapper.find("#tb-login-password").val()
                }
                self.persister.user.login(user, function () {
                    self.loadUI(selector);
                })
            });
            wrapper.on("click", "#btn-register", function () {
                var user = {
                    username: wrapper.find("#tb-register-username").val(),
                    nickname: wrapper.find("#tb-register-nickname").val(),
                    password: wrapper.find("#tb-register-password").val()
                }
                self.persister.user.register(user, function () {
                    self.loadUI(selector);
                })
            });
            wrapper.on("click", "#btn-logout", function () {
                self.persister.user.logout(function () {
                    self.loadLoginFormUI(selector);
                })
            });
            wrapper.on("click", "#btn-create-game", function () {
                var textBoxTitle = wrapper.find("#tb-create-title");
                var textBoxPassword = wrapper.find("#tb-create-pass");
                var game = {
                    title: textBoxTitle.val(),
                    password: textBoxPassword.val(),
                }
                self.persister.game.create(game, function () {
                    textBoxTitle.val("");
                    textBoxPassword.val("");
                    self.persister.game.created();
                });
            });
            wrapper.on("click", "#open-games-container a", function () {
                $("#game-join-inputs").remove();
                var gameId = $(this).parent().attr("data-game-id");
                var parentElement = $(this).parent();
                self.persister.game.isGamePassword(gameId, function (isPassword) {
                    self.loadJoinFormUI(parentElement, isPassword);
                });
            });
            wrapper.on("click", "#btn-join-game", function () {
                var password = wrapper.find("#tb-game-pass").val();
                var game = {
                    gameId: $(this).parents("li").first().attr("data-game-id")
                };
                if (password) {
                    game.password = password;
                }
                $("#game-join-inputs").remove();
                self.persister.game.join(game, function () {
                    self.persister.game.joined();
                    self.persister.game.active();
                });
            });
            wrapper.on("click", "#active-games li.game-status-Full .btn-start-game", function () {
                gameId = $(this).parent().attr("data-game-id");
                self.persister.game.start(gameId, function () {
                    self.persister.game.active();
                    self.persister.game.created();
                });
            });
            wrapper.on("click", "#game-holder #game", function (event) {
                if ($(this).hasClass("marked")) return;
                var guessModel =
                    {
                        gameId: $(this).parents("#game-holder").attr("data-game-id"),
                        position: event.target.id,
                    }
                self.persister.game.guess(guessModel);
            });
            wrapper.on("click", "#game-holder #btn-reload-game", function () {
                var gameId = $(this).parents("#game-holder").attr("data-game-id");
                self.persister.game.reload(gameId, function () {
                    self.persister.game.active();
                    self.persister.game.created();
                });
            });
            wrapper.on("click", "#active-games li.game-status-InProgress a", function () {
                var gameId = $(this).parent().attr("data-game-id");
                self.persister.game.update(gameId);
            });
            wrapper.on("click", "#created-games li.game-status-Finished a", function () {
                var gameId = $(this).parent().attr("data-game-id");
                self.persister.game.update(gameId);
            });
            wrapper.on("click", "#joined-games .btn-leave-game", function () {
                gameId = $(this).parent().attr("data-game-id");
                self.persister.game.leave(gameId, function () {
                    self.persister.game.active();
                    self.persister.game.joined();
                    var game = wrapper.find("#game-holder");
                    if (game.attr("data-game-id") == gameId) {
                        game.html("");
                        game.attr("data-game-id", "");
                        wrapper.find("#messages-holder").html("");
                    }
                });
            });
            wrapper.on("click", "#created-games .btn-delete-game", function () {
                gameId = $(this).parent().attr("data-game-id");
                self.persister.game.deleteGame(gameId, function () {
                    self.persister.game.active();
                    self.persister.game.created();

                    var game = wrapper.find("#game-holder");
                    if (game.attr("data-game-id") == gameId) {
                        game.html("");
                        game.attr("data-game-id", "");
                        wrapper.find("#messages-holder").html("");
                    }

                });
            });
        }
    });
    return {
        get: function (selector) {
            return new Controller(selector);
        }
    }
}());
