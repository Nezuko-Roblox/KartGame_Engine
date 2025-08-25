using System;

public class ServerTimeSync : TimeSync
{
	public ServerTimeSync()
	{
		this.state_ = TimeSyncState.SYNCED;
	}

	public override void Sync()
	{
	}
}
