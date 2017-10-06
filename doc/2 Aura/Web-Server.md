HTTP based services are an integral part of Mabinogi (UI saving, visual chat, shops, etc), which is why Aura comes with its own, simple web server, that handles these things.

- [General](#general)
- [URLs](#urls)
- [Services](#services)
  - [UI saving](#ui-saving)
  - [Visual chat](#visual-chat)
  - [Avatar upload](#avatar-upload)
  - [Guild management](#guild-management)
  - [Guild list](#guild-list)
  - [Wiki](#wiki)
- [Optional services](#optional-services)

##General

The web server is started together with the other servers and runs on port 80 by default, which means that you can access it by navigating your browser to http://127.0.0.1 if you're running it on your computer. However, it's possible other applications are already using port 80. Skype is one program that reserves port 80, even though it usually doesn't need it. If it, or any other applications, already use this port you have to either close them, reconfigure them (in the case of Skype you can disable the use of port 80 in the options), or you have to change the port Aura's web server is running on in the configuration, at `system/conf/web.conf`, otherwise you will get an error on start up.

If you change the port of the web server, you have to append it to the address. For example, if you changed the port to 8080, you have to use http://127.0.0.1:8080 instead of http://127.0.0.1.

##URLs

The client has a list of URLs that it uses to use the web based services, this list is located at `data/db/urls.xml`, in the extracted client data. To make the client use Aura's web server, you have to modify this file.

For example, the URL for the hotkey saving service is specified as:

```
UploadUIPage="http://mabiui.nexon.net/UiUpload.asp"
```

If you're running the web server on your computer, on the default port 80, you would change this to

```
UploadUIPage="http://127.0.0.1/upload_ui.cs"
```

the address of the UI saving service on Aura's web server.

When changing URLs, make sure you do so in the correct category. For the NA client that's `<Url Locale="usa"`.

##Services

###UI saving

The UI saving is responsible for some UI settings, like your hotkeys. Each time you log out, the client sends an XML file with these settings to the web server, and if you log in in downloads it again, without this service your hotkeys won't be saved across client restarts.

Two URLs have to be changed, the upload and the download URL, `UploadUIPage` and `DownloadUIAddress`.

**Example**

```
UploadUIPage="http://127.0.0.1/upload_ui.cs"
DownloadUIAddress="http://127.0.0.1/upload/ui/"
```

Make sure there's a trailing space at the end of `DownloadUIAddress`, otherwise it might not work.

###Visual chat

The visual chat is a feature that was never enabled in NA. It's basically a little paint tool, that allows you send images instead of chat text, which will then appear in a chat bubble above your head. To use this feature you not only have to add a URL to urls.xml (UploadVisualChatPage), but you also have to enable it in the client's features.xml file and Aura's features.txt db file.

**Example**

```
UploadVisualChatPage="http://127.0.0.1/upload_visualchat.cs"
```
```
<Feature _Name="8a124e66" _RealName="gfVisualChat" Default="G0S0" Enable="" Disable="" />
```
```
{ name: "VisualChat", enabled: true },
```

###Avatar upload

This service is possibly the simplest of all, the only thing it does is accept two files that the client uploads on each log out, `snapshot.png` and `snapshot.txt`, a screen shot of your character (the same one that gets saved to your screen shot folder) and a file containing more technical information about the character model.

To use this, change the URL UploadAvatarPage.

**Example**

```
UploadAvatarPage="http://127.0.0.1/upload_avatar.cs"
```

The files are saved at `user/save/avatar`, and could be used by CMSs for example.

###Guild management

To properly manage your guild you need access to a web page where you can remove members, accept and deny applications, set a welcome message, etc. This page can be accessed by clicking Guild Home in the Friend List, which opens "UserGuildHomePage".

**Example**

```
UserGuildHomePage="http://127.0.0.1/guild_home.cs?guildid={0}&amp;userid={1}&amp;userserver={2}&amp;userchar={3}&amp;key={4}"
```

###Guild list

The in-game guild list gets its information from an XML file generated by a web server. To make use of the guild list you have to enable it in Aura's features.txt and change the client's `GuildListPage` URL.

**Example**

```
GuildListPage="http://127.0.0.1/guild_list.cs?CharacterId={0}&amp;Name_Server={1}&amp;Page={2}&amp;SortField={3}&amp;SortType={4}&amp;GuildLevelIndex={5}&amp;GuildMemberIndex={6}&amp;GuildType={7}&amp;SearchWord={8}&amp;Passport={9}"
```

###Wiki

Unlike most other services, the Wiki page is not for the client to use, but the player. Since Aura is very easy to modify, there can be discrepancies between how quests and other features work on any given server, versus the commonly used Mabinogi World Wiki, since even if a server is on G1, that doesn't mean all G1 features are enabled, or that it's the version of G1 the player looks up.

This Wiki that comes with Aura adjusts to the activated server features, to give players detailed instructions based on whatever features were enabled/disabled. It can be viewed in the browser.

**Example**

```
http://127.0.0.1/wiki/
```

## Optional services

Optional services are scripts and pages that aren't available by default. To get more information about available ones and how to activate them, check the [web-optional](https://github.com/aura-project/aura/tree/master/system/web-optional) folder in the repository.