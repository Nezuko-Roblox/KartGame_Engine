using System;

public interface FiaEntityState
{
	bool DisplayAlert(string id);

	bool ExistsAlert();

	void Click(string id);

	void Ride(string id);

	void Unlock(string id);
}
