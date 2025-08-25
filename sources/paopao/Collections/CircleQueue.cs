using System;

public struct CircleQueue
{
	public void Initialize()
	{
		this.head = 0;
		this.tail = 0;
		this.maxNum = 0;
	}

	public void delElem()
	{
		if (this.head != this.tail)
		{
			this.head = (this.head + 1) % this.maxNum;
		}
	}

	public void addElem()
	{
		if ((this.tail + 1) % this.maxNum == this.head)
		{
			this.delElem();
		}
		this.tail = (this.tail + 1) % this.maxNum;
	}

	public int getElem(int idx)
	{
		return (this.head + idx) % this.maxNum;
	}

	public bool isFull()
	{
		return (this.tail + 1) % this.maxNum == this.head;
	}

	public bool isEmpty()
	{
		return this.head == this.tail;
	}

	public int getLastElem()
	{
		if (this.tail - 1 < 0)
		{
			return this.maxNum - 1;
		}
		return this.tail - 1;
	}

	public int getNumElem()
	{
		return (this.tail + this.maxNum - this.head) % this.maxNum;
	}

	public void setMaxNum(int num)
	{
		this.maxNum = num;
	}

	public int head;

	public int maxNum;

	public int tail;

	public int currentNum;
}
