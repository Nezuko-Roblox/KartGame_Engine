using System;

public class GameCameraman : Cameraman
{
	~GameCameraman()
	{
	}

	public void setKart(GoKart kart)
	{
		this.setKart(new GoKart[] { kart }, 1);
	}

	public void setKart(GoKart[] kart, int count)
	{
		if (count > this.m_kartCount)
		{
			this.m_kart = new GoKart[count];
		}
		for (int i = 0; i < count; i++)
		{
			this.m_kart[i] = kart[i];
		}
		this.m_kartCount = count;
		this.m_kartSelected = 0;
	}

	public virtual bool selectKart(int index)
	{
		if (this.m_kartCount > index)
		{
			this.m_kartSelected = index;
			return true;
		}
		return false;
	}

	public GoKart getKart()
	{
		if (this.m_kart != null && this.m_kartSelected < this.m_kartCount)
		{
			return this.m_kart[this.m_kartSelected];
		}
		return null;
	}

	public virtual bool stand(bool stand)
	{
		return false;
	}

	public virtual bool isStanding()
	{
		return false;
	}

	protected GoKart[] m_kart;

	protected int m_kartCount;

	protected int m_kartSelected;
}
