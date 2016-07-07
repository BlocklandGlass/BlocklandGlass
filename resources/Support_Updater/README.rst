===================================
Support_Updater Usage Documentation
===================================

Support_Updater uses a repository system to allow for easy add-on updating. This README file explains the basics of setting up your own repository and update system.

.. sectnum::
.. contents:: Table of Contents
	:backlinks: top

The Basics
----------

Every add-on requires a file called "version.txt". This file contains a version number and the URL of a repository. Support_Updater will check the repository on startup to look for a newer version. When a new version is found, the user will be notified and will be able to download the new file.
Clients will be notified of updates when running either the Blockland client or the Blockland dedicated server.

version.txt - The Version File
------------------------------

Each add-on that will work with Support_Updater must contain a text file called "version.txt". This file contains at least three important fields: version number, release channel, and repository URL.

Here is an example file::

	version	0.2.1
	channel	release
	repository	mods.greek2me.us/repository.txt
	format	TML
	id	5729

Descriptions of fields:

-	*version* is the version number of the add-on. Please see the `Version Numbering`_ section for more details.
-	*channel* is the release channel of the add-on. The channel can be named whatever is desired, but it must correspond to a channel in the repository. These are useful when creating beta or development versions of add-ons. Please see the `TML Repository Format`_ section for further details.
-	*repository* is a URL pointing to the repository where this add-on is maintained. Please see the `Repositories`_ section of this file.
-	*format* is an **optional** field that specifies the repository format. Accepted formats are TML (Torque Markup Language) and JSON. Defaults to TML if blank.
-	*id* is an **optional** field that allows add-on hosting services to specify the ID number of the add-on. This number will be sent when the repository is queried. See `Information Sent to Repository Server`_.

Alternative Format: version.json
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

Support_Updater now supports the use of JSON in version information files. Version.json files offer more options and greater flexibility than version.txt files. Here is an example JSON file::

	{
		"version":"3.2.3",
		"channel":"release",
		"repositories":
		[
			{
				"url":"http://mods.greek2me.us",
				"format":"TML"
			},
			{
				"url":"http://example.com/myRepository.php",
				"format":"JSON",
				"id":"52"
			},
			{
				"url":"http://example.org",
				"format":"TML",
				"id":"17b"
			}
		]
	}

Note that this format allows for multiple repositories to be specified. These will be tried in order until a working repository is found. This allows for a fallback in case of repository loss.

Repositories
------------

A repository is merely a text file containing TML (Torque Markup Language) or JSON tags which convey information about updates to the client. The default repository format is TML.

TML Repository Format
~~~~~~~~~~~~~~~~~~~~~

Torque Markup Language is the default repository format for legacy reasons. New repositories may use JSON instead.

This is the basic structure of a TML repository::

	<repository>
		<addon:My_AddOn>
			<channel:release>
				<version:1.7.2>
				<file:example.com/My_AddOn.zip>
			</channel>
		</addon>
	</repository>

An example of a more complicated repository::

	<repository:GreekMods>
		<addon:Support_Updater>
			<channel:release>
				<version:0.2.1>
				<restartRequired:0.2.1>
				<file:mods.greek2me.us/storage/Support_Updater/Support_Updater.zip>
				<crc:1331067106>
				<changelog:mods.greek2me.us/storage/Support_Updater/change.log>
			</channel>
		</addon>
		<addon:Gamemode_Slayer>
			<channel:release>
				<version:3.8.1>
				<restartRequired:3.8-rc-1>
				<file:mods.greek2me.us/storage/Gamemode_Slayer/Gamemode_Slayer.zip>
				<changelog:mods.greek2me.us/storage/Gamemode_Slayer/change.log>
			</channel>
			<channel:beta>
				<version:4.0-beta-3>
				<restartRequired:3.8-rc-1>
				<file:mods.greek2me.us/storage/Gamemode_Slayer/beta/Gamemode_Slayer_Beta.zip>
				<changelog:mods.greek2me.us/storage/Gamemode_Slayer/beta/change.log>
			</channel>
		</addon>
		<addon:Script_BuildCycle>
			<desc:Cycles between builds and minigames.>
			<channel:release>
				<version:0.1>
				<file:mods.greek2me.us/storage/Script_BuildCycle/Script_BuildCycle.zip>
			</channel>
		</addon>
	</repository>

Use a channel wildcard in your repository to affect all channels::

	<repository:GreekMods>
		<addon:Support_Updater>
			<channel:*>                <<< Note the asterisk
				<version:0.2.1>
				<file:mods.greek2me.us/storage/Support_Updater/Support_Updater.zip>
			</channel>
		</addon>
	</repository>

TML Repository Tags
+++++++++++++++++++

-	*addon* denotes an add-on with the name of the add-on as the first argument. For example: <addon:Support_Updater>.
-	*changelog* contains a link to the change log for this file. Please see the `Change Logs`_ section of this file for more details.
-	*channel* corresponds to the channel specified in version.txt. There may be multiple channels specified for the same add-on. See the example above.
-	*crc* is an optional field specifying a CRC value for the file. This is used to ensure that the file was downloaded properly.
-	*desc* should be used to give a brief description of the add-on and its purpose.
-	*file* contains a link to the updated ".zip" file for this add-on.
-	*repository* is the opening tag of the repository. If desired, the repository can be given a name using <repository:Your Name Here>.
-	*restartRequired* specifies the latest version that requires a restart upon updating. If a client is updating from a version below this number to a version at or above this number, a restart will be required.
-	*version* contains the version number of the latest release in that channel. See `Version Numbering`_ for more information.

JSON Repository Format
~~~~~~~~~~~~~~~~~~~~~~

The following is an example of a basic JSON repository. If using a JSON repository, be sure to set the "format" flag to "JSON" in your version.txt files.

::

	{
		"name":"GreekMods",
		"add-ons":
		[
			{
				"name":"Support_Updater",
				"description":"Support_Updater is used to update mods and stuff.",
				"channels":
				[
					{
						"name":"release",
						"version":"3.8.1",
						"restartRequired":"3.8-rc-1",
						"file":"http://mods.greek2me.us/storage/Support_Updater.zip"
					},
					{
						"name":"beta",
						"version":"4.0-beta-3",
						"restartRequired":"3.8-rc-1",
						"file":"http://mods.greek2me.us/storage/beta/Support_Updater.zip"
					}
				]
			},
			{
				"name":"Some_AddOn",
				"channels":
				[
					{
						"name":"release",
						"version":"2.7.3-rc.7",
						"file":"http://mods.greek2me.us/storage/Some_AddOn.zip",
						"changelog":"http://mods.greek2me.us/changelog/Some_AddOn.txt"
					}
				]
			},
			{
				"name":"Event_MinigameSpawn",
				"channels":
				[
					{
						"name":"*",
						"version":"1.0.0",
						"file":"http://mods.greek2me.us/storage/Event_MinigameSpawn.zip",
						"changelog":"http://mods.greek2me.us/changelog/Event_MinigameSpawn.txt"
					}
				]
			}
		]
	}

Please see the `TML Repository Tags`_ section for more details on keys such as 'version', 'restartRequired', etc.

Where to Host Your Repository
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

Repositories can be hosted on any website that provides a direct link to your plain-text file. However, Blockland/Torque only supports HTTP. **Do not use websites that require HTTPS.** This also applies to add-on .zip files and change logs.

If desired, the repository URL can be made much cleaner by setting it as the index file of a website. For example, mods.greek2me.us/repository.txt can be simplified to mods.greek2me.us.

Information Sent to Repository Server
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

Support_Updater automatically passes a GET variable called 'user' to the repository, which contains the username and can be used for statistics.

Additionally, a list of add-ons belonging to the repository that have the ID flag set in their version.txt file will be sent via a GET variable called 'mods' in a dash-delimited format, like this example::

	4634-3146-3155-5464-2144

Each number in the above example signifies an add-on ID.

Version Numbering
-----------------

Support_Updater is fully compliant with the `Semantic Versioning`_ standard. Your version numbers must use this system.

.. _`Semantic Versioning`: http://semver.org

Change Logs
-----------

Change logs are stored in a separate file from the repository. The repository references them using the <changlog> tag. See `TML Repository Tags`_ for more information.

Formatting
~~~~~~~~~~

Change logs are formatted using Support_TMLParser TML formatting. This is merely an enhanced version of Blockland's default TML formatting.

Here is an example of an extremely basic change log::

	<version:1.8>
		<ul>
			<li>Added stuff.</li>
			<li>Changed some stuff.</li>
		</ul>
	</version>
	<version:1.7.5>
		<ul>
			<li>Some other changes.</li>
			<ol>
				<li>It's an ordered list inside an unordered one!</li>
			</ol>
		</ul>
	</version>

**List of common TML tags:**

================================  =============================================
             Tag                                     Description
================================  =============================================
<b>...</b>                        Bold text
<i>...</i>                        Italicized text
<u>...</u>                        Underlined text
<font:Arial:24>                   Change the font and size
<size:24>...</size>               Change the font size
<color:ff0000>...</color>         Change the text color
<just:center>...</just>           Justify the text (left/right/center)
<h1>...</h1>                      Heading 1
<h2>...</h2>                      Heading 2
<h3>...</h3>                      Heading 3
<ol>...</ol>                      Ordered list
<ul>...</ul>                      Unordered list
<li>...</li>                      List item
<a:url>title</a>                  Hyperlink (do not include *http://*)
<version:1.8>                     Shortcut to typing <h3>Version 1.8</h3>
================================  =============================================

**Additional TML tags** can be viewed here_.

.. _here: http://gist.io/8103673

Update Scripts/Update Callbacks
-------------------------------

To run a script after the update has downloaded, simply create a text file called "update.cs" and place it in your add-on. Support_Updater will automatically execute it after the update has completed.

Support_Updater creates variables for use in update scripts::

	$version__AddOn_Name //The version that was just installed.
	$versionOld__AddOn_Name //The previous version number.
	$versionRestartRequired__AddOn_Name // Whether this update requires a restart.

About Support_Updater
---------------------

Created by Greek2me. (BLID 11902)

Contact
~~~~~~~

 - `Blockland Forums <http://forum.blockland.us/index.php?action=profile;u=22331>`_
 - `Email <greektume@gmail.com>`_
