# Toby (ReplyBot)

A Discord bot for responding to you with messages based on triggers.

Now written in C# and .NET 6.

Previous Version: https://github.com/rarDevelopment/replybot

# Toby's Names

Toby answers to the following names:
- toby
- replybot
- responsebot
- tobias
- tobster

Toby will also answer to whatever his nickname is in your server.

# Configuration Settings

## Avatar Change Announcements

Avatar change announcements will post in the server's system channel (where welcome messages are posted) when a user in the server updates their avatar. This includes both user avatar changes and server profile avatar changes. If you're an administrator in the server, you can enable or disable avatar announcements by using the `/set-avatar-announcement` command.

## Avatar Change Mentions

This relates to the avatar change announcements above. You can also have the user who updated their avatar be @mentioned in those announcements. You can enable or disable these mentions with the `/set-avatar-mention` command.

## Server Activity Logging

For moderators, you might want to keep a more detailed log of the activity on your server than the default audit log functionality in Discord is capable of doing. You can have Toby log server activity to a particular channel by using the `/set-logs-channel` command and specifying the channel where you want the logs to appear.

## View Settings

Use `/view-settings` to see the existing settings for all of the above options.

# Useful Functions

## Preview Discord Message Links

If you post a link to another message in the same server, Toby will preview that message for you, which can often save you a click.

## Define Words (uses [Free Dictionary API](https://github.com/meetDeveloper/freeDictionaryAPI))

You can say `Toby define [word here]` and he'll try to find a dictionary definition for that word.

## Simple Polls Using Reactions

You can make a (very simple) poll by doing `"Toby poll [question here], [answer 1 here], [answer 2 here], and so on..."`. This will genereate a post with the question, answer choices, and reactions associated with each answer choice.

## Fix Tweet Previews (uses [FixTweet](https://github.com/FixTweet/FixTweet))
If you say `Toby fix tweet [your tweet link here]` he will reply with that same link at fxtwitter.com, which is a site that fixes video and image preview embeds in Discord's tweet previews. This works with all tweets in the message.

If you reply to another message with a tweet link in it and say `Toby fix tweet`, he'll fix those tweets for you.

## Game Length Estimates (using [HowLongToBeat](https://howlongtobeat.com/))

Use `Toby hltb [game here]` and Toby will tell you how long that game is.

## Find Where Movies/TV Shows Are Streaming (using [JustWatch](https://justwatch.com))

If you're looking to watch something, `Toby can I stream [movie or show here]` will give you US, Canada, and UK links to justWatch, where you can find where it's streaming (if at all). If you need another location, you can change the country on the site itself.

## Find a Song or Album Link on All Services (using [Odesli](https://odesli.co/))

If you have a song or album link on Apple Music, Spotify, YouTube Music, etc. but you'd like to share it to a server where your friends might use another streaming service, say `Toby songlink [link to the song/album here]` and Toby will generate a link that will show you all of the services where that song or album can be found.

## Fortnite Shop Information (using [Fortnite API](https://fortnite-api.com/))
If you're a Fortnite player, "Toby Fortnite shop" will show you a glimpse of what's on the shop currently

# Fun Stuff

## Flip a Coin

Make those challenging decisions - say `Toby flip a coin` and he'll do so.

## Search Something Real Quick

Toby can grab a quick link to some search results for you, just say `Toby search [search term here]`. Toby prefers DuckDuckGo 🦆 so if you'd like to use Google specifically, say `Toby google [search term here]`.

## 8-Ball

Want to shake the 8-ball? Ask Toby a question and include `Toby 🎱` or `Toby 8-ball` and you'll get an 8-ball style response, as if you shook it yourself.

## Repeat After Me

Toby can say what you want, just say `toby say [what you want him to say here]`.
