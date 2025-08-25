using System;

public class ArrayEx<T>
{
	public ArrayEx(int size)
	{
		this.begin_ = 0;
		this.length_ = size;
		this.data_ = new T[size];
	}

	public void Copy(T[] data)
	{
		this.begin_ = 0;
		this.length_ = data.Length;
		if (this.data_ == null)
		{
			this.data_ = new T[this.length_];
		}
		Array.Copy(data, this.data_, this.length_);
	}

	public T this[int i]
	{
		get
		{
			return this.data_[this.begin_ + i];
		}
		set
		{
			this.data_[this.begin_ + i] = value;
		}
	}

	public int Length
	{
		get
		{
			return this.length_;
		}
	}

	public int begin_;

	public int length_;

	public T[] data_;
}
