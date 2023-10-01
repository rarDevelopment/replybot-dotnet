# Toby (ReplyBot)

A Discord bot for responding to you with messages based on triggers.

Now written in C# and .NET 7.

Previous Version: https://github.com/rarDevelopment/replybot

[Invite Toby to your Discord Server](https://discord.com/api/oauth2/authorize?client_id=737404113624498347&permissions=423792999488&scope=bot%20applications.commands)

[Discord Support Server](https://discord.gg/Za4NAtJJ9v)

![Toby](https://user-images.githubusercontent.com/4060573/222975221-94dff40a-207d-4c34-b633-7d74cffb8d19.png)

# Toby's Names

Toby answers to the following names:

- toby
- replybot
- tobias
- tobster

Toby will also answer to whatever his nickname is in your server.

# Configuration Settings

## Avatar Change Announcements

Avatar change announcements will post in the server's system channel (where welcome messages are posted) when a user in the server updates their avatar. This includes both user avatar changes and server profile avatar changes. If you're an administrator in the server, you can enable or disable avatar announcements by using the `/set-avatar-announcement` command.

## Avatar Change Mentions

This relates to the avatar change announcements above. You can also have the user who updated their avatar be @mentioned in those announcements. You can enable or disable these mentions with the `/set-avatar-mention` command.

## Default Replies

By default, Toby has replies to give him some personality. However, if you'd like to turn this off, you can use the `/set-default-replies` command.

## Server Activity Logging

For moderators, you might want to keep a more detailed log of the activity on your server than the default audit log functionality in Discord is capable of doing. You can have Toby log server activity to a particular channel by using the `/set-logs-channel` command and specifying the channel where you want the logs to appear.

## View Settings

Use `/view-settings` to see the existing settings for all of the above options.

# Useful Functions

## Preview Discord Message Links

If you post a link to another message in the same server, Toby will preview that message for you, which can often save you a click.

## Identifies Previously Posted Links

If you post a link to that has already been posted recently in the same channel, Toby will notify you of this and link you to the previous post. This can help catch up on any discussion that already happened for that link.

## Define Words (uses [Free Dictionary API](https://github.com/meetDeveloper/freeDictionaryAPI))

You can say `Toby define [word here]` and he'll try to find a dictionary definition for that word.

## Simple Polls Using Reactions

You can make a (very simple) poll by doing `"Toby poll [question here], [answer 1 here], [answer 2 here], and so on..."`. This will genereate a post with the question, answer choices, and reactions associated with each answer choice.

## Fix Tweet Previews ~~(uses [FixTweet](https://github.com/FixTweet/FixTweet))~~ (uses [BetterTwitFix](https://github.com/ryuuzake/BetterTwitFix))

Tweets posted in Discord will include a preview embed of that tweet. But if there's a video, that video preview might not play properly. Or if there is more than 1 image, those other images won't show on mobile devices. To help with this, Toby will add a reaction emoji to any post that includes a Twitter link. If someone taps on that reaction emoji, Toby will reply with that same link at fxtwitter.com, which is a site that fixes video and image preview embeds in Discord's tweet previews. This works with all tweets in the message. If someone has posted an vxtwitter.com link directly and you'd like the original tweet link, this will work the same way - a reaction will appear and tapping the reaction will lead Toby to reply with the original tweet link. Admins can use the `/set-fix-tweet-reactions` command to turn this on or off.

## View Tweets Without An Account (uses [Nitter](https://github.com/zedeus/nitter))

Nitter is a site that pulls in Twitter's data to display separate from Twitter. This means you can view tweets and tweet threads without an account, despite newly-added restrictions on Twitter.com.

## Fix Instagram Previews (uses [InstaFix](https://github.com/Wikidepia/InstaFix))

Instagram links posted in Discord sometimes don't preview at all! Similar to the above, Toby will add a reaction emoji to any post that includes an Instagram link. If someone taps that reaction emoji, Toby will reply with that same link at ddinstagram.com, which is a site that fixes video and image preview embeds in Discord's Instagram previews. This works with all Instagram posts in the message. If someone has posted a ddinstagram.com link directly and you'd like the original link, this will work the same way - a reaction will appear and tapping the reaction will lead Toby to reply with the original Instagram link. Admins can use the `/set-fix-instagram-reactions` command to turn this on or off.

## Fix Bluesky Previews

Instagram links posted in Discord don't preview at all! Similar to the above, Toby will add a reaction emoji to any post that includes an Bluesky post link. If someone taps that reaction emoji, Toby will reply with the text and images (including alt text) from that post. This works with all Bluesky posts in the message. Admins can use the `/set-fix-bluesky-reactions` command to turn this on or off.

## Video Game Length Estimates (using [HowLongToBeat](https://howlongtobeat.com/))

Use `Toby hltb [game here]` and Toby will tell you how long that game is.

## Video Game Release Dates (using [IGDB](https://www.igdb.com/))

Use `Toby when did [game here] come out` or `Toby when will [game here] release?` (or a few other possible wordings) and Toby will tell you the release date(s) that game.

## Movie Runtimes (using [The Movie Database (TMDB)](https://themoviedb.org/))

Use `Toby how long is [movie here]` or `Toby how long to watch [movie here]` or `Toby hltw [movie here]` and Toby will tell you how long that movie is (if that information is present in TMDB).

## Find Where Movies/TV Shows Are Streaming (using [JustWatch](https://justwatch.com))

If you're looking to watch something, `Toby can I stream "[movie or show here]" in [country here]` will give you links to JustWatch for your specified country, where you can find where it's streaming (if at all). If you need a location that is not supported, you can request it by opening an issue on [this repo](https://github.com/rarDevelopment/justwatch-country-config) or you can change the country on the site itself.

## Find a Song or Album Link on All Services (using [Odesli](https://odesli.co/))

If you have a song or album link on Apple Music, Spotify, YouTube Music, etc. but you'd like to share it to a server where your friends might use another streaming service, say `Toby songlink [link to the song/album here]` and Toby will generate a link that will show you all of the services where that song or album can be found.

## Fortnite Shop Information (using [Fortnite API](https://fortnite-api.com/))

If you're a Fortnite player, "Toby Fortnite shop" will show you a glimpse of what's on the shop currently

## Flip a Coin

Make those challenging decisions - say `Toby flip a coin` and he'll do so.

## Choose For Me

Make _more_ challenging decisions - say something like `"Toby pick one for me: [option 1], [option 2], [option 3], and so on..."` and he'll pick one of the comma-separated options for you. You can trigger this with a variety of phrases, like `Toby choose:` or `Toby please select one for me:` or `Toby random:`, but the colon `:` is important!

## Get a Quick Search Link

Toby can grab a quick link to some search results for you, just say `Toby google [search term here]` for Google, `Toby ddg [search term here]` for DuckDuckGo (full name also works), or `Toby bing [search term here]` for Bing.

## 8-Ball

Want to shake the 8-ball? Ask Toby a question and include `Toby 🎱` or `Toby 8-ball` and you'll get an 8-ball style response, as if you shook it yourself.

## Repeat After Me

Toby can say what you want, just say `toby say [what you want him to say here]`.
