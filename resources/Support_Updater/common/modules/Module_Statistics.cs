//----------------------------------------------------------------------
// Title:   Module_Statistics
// Author:  Greek2me
// Version: 1
// Updated: January 14, 2016
//----------------------------------------------------------------------
// Sends player statistics to greek2me.us.
//----------------------------------------------------------------------
// Include this code in your own scripts as an *individual file*
// called "Module_Statistics.cs". Do not modify this code.
//----------------------------------------------------------------------

if($Module_Statistics::Version >= 1)
	return;
$Module_Statistics::Version = 1;

if($Pref::Server::DisableStatistics)
	return;

$Statistics::PostTimeOut = 10000;
$Statistics::PostQueueLen = 0;
$Statistics::ServerAddress = "mods.greek2me.us";
$Statistics::ServerPort = "80";
$Statistics::ServerPath = "/statistics/bl-users-post.php";

function statsAddToPostQueue(%name, %blid, %ip)
{
	$Statistics::PostQueue[$Statistics::PostQueueLen, "name"] = %name;
	$Statistics::PostQueue[$Statistics::PostQueueLen, "addr"] = %ip;
	$Statistics::PostQueueLen ++;
	cancel($Statistics::PostSched);
	$Statistics::PostSched = schedule($Statistics::PostTimeOut, 0, statsPostAll);
}

function statsPostAll()
{
	cancel($Statistics::PostSched);
	for(%i = 0; %i < $Statistics::PostQueueLen; %i ++)
	{
		%name = urlEnc($Statistics::PostQueue[%i, "name"]);
		%ip = $Statistics::PostQueue[%i, "addr"];
		%data = setRecord(%data, %i, %name @ ";" @ %ip);
	}
	%tcp = TCPClient
	(
		"POST",
		$Statistics::ServerAddress,
		$Statistics::ServerPort,
		$Statistics::ServerPath,
		%data
	);
	deleteVariables("$Statistics::PostQueue*");
	$Statistics::PostQueueLen = 0;
}

package Module_Statistics
{
	function GameConnection::autoAdminCheck(%this)
	{
		%blid = %this.getBLID();
		if(%blid !$= getNumKeyID() && !$Server::LAN)
		{
			%ip = %this.isLAN() ? "LOCAL" : %this.getRawIP();
			statsAddToPostQueue(%this.getPlayerName(), %blid, %ip);
		}
		return parent::autoAdminCheck(%this);
	}
};
activatePackage(Module_Statistics);