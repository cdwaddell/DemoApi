var meetupUrl = 'https://api.meetup.com/find/upcoming_events?&sign=false&photo-host=public&lon=' + longitude + '&topic_category=292&order=time&page=100&lat=' + latitude + '&callback=populateNCAA&key=5b1522805118242d49403d78291a88';
var sportRadarUrl = http://api.sportradar.us/ncaamb/trial/v4/en/tournaments/54d32815-c385-4266-8248-34b1b22fe2f7/schedule.json?api_key=rku363fn5vrbb7sh963ncpxa;

if ("geolocation" in navigator) {
	navigator.geolocation.getCurrentPosition(function(position) {
		PopulateEvents(position.coords.latitude, position.coords.longitude);
	});
} else {
  PopulateEvents(38.25, -85.78);
}

function PopulateEvents(latitude, longitude)
{
	var script = $("<script />", {
		src: meetupUrl
	  }
	);

	$("body").append(script);
}

function populateNCAA(response){	
    sportRadarUrl = 'ncaa.json';
    var $app = $('#app');

    var l = 0;
	$.ajax({
		url: sportRadarUrl
    }).done(function (data) {

        var events = response.data.events;

        for (var i = 0; i < data.rounds.length; i++) {
            var round = data.rounds[i];
            var brackets = round.bracketed;
            for (var j = 0; j < brackets.length; j++) {
                var games = brackets[j].games;
                for (var k = 0; k < games.length; k++) {
                    var game = games[k];

                    var start = new Date(game.scheduled);
                    var end = new Date(start.getTime());
                    end.setHours(end.getHours() + 2);

                    while (true) {
                        var event = events[l];
                        var eventstart = new Date(event.time);
                        var eventend = new Date(event.time + event.duration);
                        if (l > events.legnth) break;
                        if (eventend < start) {
                            l = l + 1;
                        }
                        else if (
                            start < eventstart && end > eventstart ||
                            start < eventend && start > eventend ||
                            eventstart < start && eventend > start ||
                            eventstart < end && eventend > end
                        ) {
                            var newDiv = '<div class="card"><div class="card-body">' + game.away.name + '<br />@<br />' + game.home.name + '<br />vs<br/>'+event.name+'</div></div>';
                            $app.append(newDiv)
                            break;
                        }
                        else
                            break;
                    }
                }
                
            }
        }
	})
}