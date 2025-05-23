# Toby (ReplyBot)

A Discord bot for responding to you with messages based on triggers.

Now written in C# and .NET 9.

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

Avatar change announcements will post in the server's system channel (where Discord's default welcome messages are posted) when a user in the server updates their avatar. This includes both user avatar changes and server profile avatar changes. If you're an administrator in the server, you can enable or disable avatar announcements by using the `/set-avatar-announcement` command.

## Avatar Change Mentions

This relates to the avatar change announcements above. You can also have the user who updated their avatar be @mentioned in those announcements. You can enable or disable these mentions with the `/set-avatar-mention` command. You can also choose to omit certain users (including bots) from these announcements by using the `/set-user-avatar-ignore` command.

## Default Replies

By default, Toby has replies to give him some personality. However, if you'd like to turn this off, you can use the `/set-default-replies` command.

## Server Activity Logging

For moderators, you might want to keep a more detailed log of the activity on your server than the default audit log functionality in Discord is capable of doing. You can have Toby log server activity to a particular channel by using the `/set-logging` command and specifying the channel where you want the logs to appear. You can individually specify to log user server joins, user server departures, user bans, user unbans, message edits, and message deletes.

## Welcome Messages

When a user joins the server, welcome messages will post in the server's system channel (where Discord's default welcome messages are posted). If you're an administrator in the server, you can enable or disable welcome messages by using the `/set-welcome-message` command.

## Departure Messages

When a user leaves the server, departure messages can be posted in the server's system channel (where Discord's default welcome messages are posted). If you're an administrator in the server, you can enable or disable departure messages by using the `/set-departure-message` command. This setting defaults to off.

## View Settings

Use `/view-settings` to see the existing settings for all of the above options.

# Useful Functions

## Preview Discord Message Links

If you post a link to another message in the same server, Toby will preview that message for you, which can often save you a click.

## Identifies Previously Posted Links

If you post a link to that has already been posted recently in the same channel, Toby will notify you of this and link you to the previous post. This can help catch up on any discussion that already happened for that link. You can enable or disable this setting by using the `/set-repeat-links` command. This setting defaults to on.

## Define Words (uses [Free Dictionary API](https://github.com/meetDeveloper/freeDictionaryAPI))

You can say `Toby define [word here]` and he'll try to find a dictionary definition for that word.

## Fix Tweet Previews (uses [BetterTwitFix](https://github.com/ryuuzake/BetterTwitFix))

Tweets posted in Discord will include a preview embed of that tweet. But if there's a video, that video preview might not play properly. Or if there is more than 1 image, those other images won't show on mobile devices. To help with this, Toby will add a reaction emoji to any post that includes a Twitter link. If someone taps on that reaction emoji, Toby will reply with that same link at fxtwitter.com, which is a site that fixes video and image preview embeds in Discord's tweet previews. This works with all tweets in the message. If someone has posted an vxtwitter.com link directly and you'd like the original tweet link, this will work the same way - a reaction will appear and tapping the reaction will lead Toby to reply with the original tweet link. Admins can use the `/set-fix-tweet-reactions` command to turn this on or off.

## Fix Instagram Previews (uses [InstaFix](https://github.com/Wikidepia/InstaFix))

Instagram links posted in Discord sometimes don't preview at all! Similar to the above, Toby will add a reaction emoji to any post that includes an Instagram link. If someone taps that reaction emoji, Toby will reply with that same link at ddinstagram.com, which is a site that fixes video and image preview embeds in Discord's Instagram previews. This works with all Instagram posts in the message. If someone has posted a ddinstagram.com link directly and you'd like the original link, this will work the same way - a reaction will appear and tapping the reaction will lead Toby to reply with the original Instagram link. Admins can use the `/set-fix-instagram-reactions` command to turn this on or off.

## Fix Threads Previews (uses [fixthreads](https://github.com/milanmdev/fixthreads))

Similar to the above, Toby will add a reaction emoji to any post that includes a Threads link. If someone taps that reaction emoji, Toby will reply with that same link at fixthreads.net, which is a site that fixes video and image preview embeds in Discord's Instagram previews. This works with all Threads posts in the message. If someone has posted a fixthreads.net link directly and you'd like the original link, this will work the same way - a reaction will appear and tapping the reaction will lead Toby to reply with the original Threads link. Admins can use the `/set-fix-threads-reactions` command to turn this on or off.

## Fix Bluesky Previews

Similar to the above, Toby will add a reaction emoji to any post that includes a Bluesky post link. If someone taps that reaction emoji, Toby will reply with the text and images (including alt text) from that post, as well as a note if there's a quoted post. This works with all Bluesky posts in the message. Admins can use the `/set-fix-bluesky-reactions` command to turn this on or off.

## Fix Reddit Previews (uses [vxReddit](https://github.com/dylanpdx/vxReddit))

Similar to the above, Toby will add a reaction emoji to any post that includes a Reddit link. If someone taps that reaction emoji, Toby will reply with that same link at ddinstagram.com, which is a site that fixes video and image preview embeds in Discord's Instagram previews. This works with all Instagram posts in the message. If someone has posted a vxreddit.com link directly and you'd like the original link, this will work the same way - a reaction will appear and tapping the reaction will lead Toby to reply with the original Reddit link. Admins can use the `/set-fix-instagram-reactions` command to turn this on or off.

## Fix TikTok Previews

Similar to the above, Toby will add a reaction emoji to any post that includes a TikTok post link. If someone taps that reaction emoji, Toby will reply with an updated link that should include a functioning preview for the linked TikTok video. This works with all TikTok links in the message. Admins can use the `/set-fix-tiktok-reactions` command to turn this on or off.

## Video Game Length Estimates (using [HowLongToBeat](https://howlongtobeat.com/))

Use `Toby hltb [game here]` or `Toby how long to beat [game here]` and Toby will tell you how long that game is.

## Video Game Release Dates (using [IGDB](https://www.igdb.com/))

Use `Toby when did [game here] come out` or `Toby when will [game here] release?` (or a few other possible wordings) and Toby will tell you the release date(s) that game.

## Movie Runtimes (using [The Movie Database (TMDB)](https://themoviedb.org/))

Use `Toby how long is [movie here]` or `Toby how long to watch [movie here]` or `Toby hltw [movie here]` and Toby will tell you how long that movie is (if that information is present in TMDB).

## Find Where Movies/TV Shows Are Streaming (using [JustWatch](https://justwatch.com))

If you're looking to watch something, `Toby can I stream [movie or show here] in [country here]` will give you links to JustWatch for your specified country, where you can find where it's streaming (if at all). If you need a location that is not supported, you can request it by opening an issue on [this repo](https://github.com/rarDevelopment/justwatch-country-config) or you can change the country on the site itself.

## Find a Song or Album Link on All Services (using [Odesli](https://odesli.co/))

If you have a song or album link on Apple Music, Spotify, YouTube Music, etc. but you'd like to share it to a server where your friends might use another streaming service, say `Toby songlink [link to the song/album here]` and Toby will generate a link that will show you all of the services where that song or album can be found.

## Existing Emotes (Add to your Server or Get a Link to the Image)

If you have a custom emote you like, you can use `Toby emote [emote(s) here]` and Toby will give you a link to that emote as an image, which you can then save. If someone's used the emote(s) and you just want to reference their message, you can just reply to them and say `Toby emote`. If you have permission to manage emotes in the server, you can also say `Toby add emote [emote(s) here]` (or reply to the message with the emote(s) as mentioned previously) and it will also be added to the server.

## Create New Emotes

If you have an image you'd like to make into an emote, you can use `Toby add emote` and include the image attached to the message and Toby will create the emote with that image. You can also say something like `Toby add emote bob 123` and Toby will create the emote with the name `Bob123`. If a name isn't specified, Toby will use the file name (without the extension) as the emote name.

## Stickers (Add to your Server or Get a Link to the Image)

If you have a sticker you like, you can reply to a message with a sticker and say `Toby sticker` and Toby will give you a link to that sticker as an image, which you can then save. If you have permission to manage stickers in the server, you can also reply and say `Toby add sticker` and it will also be added to the server.

## Fortnite Map Location (using [Fortnite API](https://fortnite-api.com/))

Again, if you're a Fortnite player, `Toby where we droppin` or `Toby wwd` will randomly choose a location from the current Fortnite Battle Royale map for you to drop at. You can set this command to only use named locations using the `/set-fortnite-named-locations` command.

## Fortnite User Stats (using [Fortnite API](https://fortnite-api.com/))

Again, if you're a Fortnite player, `Toby fortnite stats [Fortnite username]` will give you stats for each team size in Fortnite.

## Flip a Coin

Make those challenging decisions - say `Toby flip a coin` and he'll do so.

## Choose For Me

Make _more_ challenging decisions - say something like `"Toby pick: [option 1], [option 2], [option 3], ..."` and he'll pick one of the options for you. You can also specify how many options you want chosen by sasying `"Toby pick 3: [option 1], [option 2], [option 3], [option 4]"` and he'll pick 3 of those options at random. You can trigger this with a variety of phrases, like `Toby choose:` or `Toby please select 1:` or `Toby random:` or `Toby decide 4:`, but the colon `:` is important! You can choose to separate your options with commas, spaces, or pipes (|). Finally, you can also specify voice channels instead of options and Toby will pick the number of users you specify from the users connected to those voice channels.

## Get a Quick Search Link

Toby can grab a quick link to some search results for you, just say `Toby google [search term here]` for Google, `Toby ddg [search term here]` for DuckDuckGo (full name also works), or `Toby bing [search term here]` for Bing.

## 8-Ball

Want to shake the 8-ball? Ask Toby a question and include `Toby 🎱` or `Toby 8-ball` and you'll get an 8-ball style response, as if you shook it yourself.

## Repeat After Me

Toby can say what you want, just say `toby say [what you want him to say here]`.
