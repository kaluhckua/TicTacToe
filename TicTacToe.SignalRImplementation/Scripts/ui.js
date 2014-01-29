/// <reference path="jquery-2.0.3.js" />

var ui = (function () {

    function buildGameUI(nickname) {
        var html = '<span id="user-nickname">' +
				nickname +
		'</span>' +
		'<button id="btn-logout">Logout</button><br/>' +
		'<div id="create-game-holder">' +            
			'Title: <input type="text" id="tb-create-title" />' +
			'Password: <input type="password" id="tb-create-pass" />' +
			'<button id="btn-create-game">Create</button>' +           
		'</div>' +      
        '<div id="games-list-container">' +
            '<ul>' +
                 '<li><a href="#created-games-container">Created games</a></li>' +
                 ' <li><a href="#open-games-container">Open</a></li>' +
                 ' <li><a href="#joined-games-container">Joined</a></li>' +
                 ' <li><a href="#active-games-container">Active</a></li>' +
             '</ul>' +
                '<div id="created-games-container" class="games-list">' +			      
			        '<div id="created-games" ></div>' +
		        '</div>' +
		        '<div id="open-games-container" class="games-list">' +			       
			        '<div id="open-games"></div>' +
		        '</div>' +
                '<div id="joined-games-container" class="games-list">' +                 
                    '<div id="joined-games" >' +
                     '</div>' +
                '</div>' +
                 '<div id="active-games-container" class="games-list">' +			     
			        '<div id="active-games" ></div>' +
		        '</div>' +
            
        '</div>' +
		'<div id="game-holder" data-game-id="">' +
		'</div>' +
		'<div id="messages-holder">' +
		'</div>'+
        '<div id="error-message-holder">' +
		'</div>';
        return html;


    }
    function buildLoginForm() {
        var html =
             '<div id="login-form-holder">' +
				'<form>' +
					'<div id="login-form">' +
                         '<fieldset>' +
						'<label for="tb-login-username">Username: </label>' +
						'<input type="text" id="tb-login-username"><br />' +
						'<label for="tb-login-password">Password: </label>' +
						'<input type="password" id="tb-login-password"><br />' +
						'<button id="btn-login" type="button" class="button">Login</button>' +
					'</div>' +
					'<a href="#" id="btn-show-register" class="button">Register</a>' +
                     '</fieldset>' +
				'</form>' +
				'<div id="error-messages"></div>' +
            '</div>'+
             '<div id="error-message-holder">' +
		    '</div>';
        return html;
    }
    function bulidRegisterForm() {
        var html =
            '<form>' +
                '<div id="register-form">' +
                  '<fieldset>' +
						'<label for="tb-register-username">Username: </label>' +
						'<input type="text" id="tb-register-username"><br />' +
						'<label for="tb-register-nickname">Nickname: </label>' +
						'<input type="text" id="tb-register-nickname"><br />' +
						'<label for="tb-register-password">Password: </label>' +
						'<input type="password" id="tb-register-password"><br />' +
						'<button id="btn-register" class="button">Register</button>' +
					'</div>' +
                    '<a href="#" id="btn-show-login" class="button selected">Login</a>   ' +
                  '</fieldset>' +
        '</form>'+
        '<div id="error-message-holder">' +
        '</div>';
        return html;

    }
    function buildJoinForm(isPassword) {
        var html =
				'<div id="game-join-inputs">';
        if (isPassword == true) {
            html += 'Password: <input type="password" id="tb-game-pass"/>';
        }
        html += '<button id="btn-join-game">join</button>' +
                  '</div>';
        return html
    }
    function buldTicTacToeUI(gameState) {

        var html = '<div id="game-information">' +
                         '<div id="title">Title: ' + gameState.Title +
                          '</div>' +
                        '<div id="opponent">' +
                              'You are playing against:' + gameState.Opponent +
                        '</div>' +
                        '<div id="symbol">' +
                              'You play with: <img src=';
        if (gameState.Symbol == "O") {
            html += "/Content/Images/TicTacToeO.png";
        }
        else {
            html += "/Content/Images/TicTacToeX.png";
        }
        html += ' height="20" width="20" style="vertical-align:bottom"/>' +
                         '</div>' +
                         '<div id="on-turn"></div>' +
                         '<div id="message"></div>' +
                         '<div id="reload-game"><button id="btn-reload-game" style="display:none">Reload</button></div>' +
                     '</div>' +
                     '<div id="game">';
        for (var i = 1; i <= 9; i++) {
            html += ("<span id=" + i + " class='box' />");
        }
        html += '</div>';
       
        return html;
    }
    function buildOpenGamesList(games) {
        var list = '<ul class="games-list open-games">';
        for (var i = 0; i < games.length; i++) {
            var game = games[i];
            list +=
				'<li data-game-id="' + game.Id + '">' +
					'<a href="#" >' +
						$("<div />").html(game.Title).text() +
					'</a>' +
					'<span> by ' +
						game.CreatorNickname +
					'</span>' +
				'</li>';
        }
        list += "</ul>";
        return list;
    }
    function buildCreatedGamesList(games) {
        var list = '<ul class="games-list created-games">';
        for (var i = 0; i < games.length; i++) {
            var game = games[i];
            list +=
				'<li class="game-status-' + game.Status + '" data-game-id="' + game.Id + '" data-creator="' + game.CreatorNickname + '">' +
					'<a href="#">' + game.Title +
					'</a>' +
					'<span> by ' +
						game.CreatorNickname +
					'</span>' +
                    '     Status:<span>  ' +
						game.Status +
					'</span>'+
                    '<button class="btn-delete-game"></button>'+
				'</li>';
        }
        list += "</ul>";
        return list;

    }
    function buildActiveGamesList(games, nickname) {
        var gamesList = Array.prototype.slice.call(games, 0);
        gamesList.sort(function (g1, g2) {
            if (g1.status == g2.status) {
                return g1.title > g2.title;
            }
            else {
                if (g1.status == "InProgress") {
                    return -1;
                }
            }
            return 1;
        });
        var list = '<ul class="games-list active-games">';
        for (var i = 0; i < gamesList.length; i++) {
            var game = gamesList[i];
            list +=
				'<li class="game-status-' + game.Status + '" data-game-id="' + game.Id + '" data-creator="' + game.CreatorNickname + '">' +
					'<a href="#">' + game.Title +
					'</a>' +
					'<span> by ' +
						game.CreatorNickname +
					'</span>' +
                    '     Status:<span>  ' +
						game.Status +
					'</span>';
            if (game.Status == "Full") {
                if (game.CreatorNickname == nickname) {
                    list += '<button  class="btn-start-game">Start</button>';
                }
                else {
                    list += '   Waiting to start'
                }
            }

            list += '</li>';
        }
        list += "</ul>";
        return list;
    }
    function buildJoinedGamesList(games) {
        var list = '<ul class="games-list joined-games">';
        for (var i = 0; i < games.length; i++) {
            var game = games[i];
            list +=
				'<li data-game-id="' + game.Id + '" data-game-status="' + game.Status + '"> ' +
					'<a href="#" >' +
						$("<div />").html(game.Title).text() +
					'</a>' +
					'<span> by ' +
						game.CreatorNickname +
					'</span>' +
                    '<button class="btn-leave-game">Leave</button>'+
				'</li>';
        }
        list += "</ul>";
        return list;
    }
   
    return {
        loginForm: buildLoginForm,
        registerForm: bulidRegisterForm,
        gameUI: buildGameUI,
        openGamesList: buildOpenGamesList,
        activeGamesList: buildActiveGamesList,
        createdGamesList: buildCreatedGamesList,
        joinedGamesList:buildJoinedGamesList,
        joinForm: buildJoinForm,
        ticTacToe: buldTicTacToeUI,
    }
}())