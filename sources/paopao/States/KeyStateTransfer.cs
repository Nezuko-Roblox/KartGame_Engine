using System;

public class KeyStateTransfer
{
	public static KeyState GetKeyState(KeyState src, bool isPush)
	{
		return KeyStateTransfer.KEY_STATE_TRANSFER[(int)src].GetKeyState(isPush);
	}

	private static KeyStateTransfer.KeyStateTransferElem[] KEY_STATE_TRANSFER = new KeyStateTransfer.KeyStateTransferElem[]
	{
		new KeyStateTransfer.KeyStateTransferElem(KeyState.PUSH, KeyState.NONE),
		new KeyStateTransfer.KeyStateTransferElem(KeyState.PRESS, KeyState.RELEASE),
		new KeyStateTransfer.KeyStateTransferElem(KeyState.PRESS, KeyState.RELEASE),
		new KeyStateTransfer.KeyStateTransferElem(KeyState.PUSH, KeyState.NONE)
	};

	public class KeyStateTransferElem
	{
		public KeyStateTransferElem(KeyState push, KeyState release)
		{
			this.push_ = push;
			this.release_ = release;
		}

		public KeyState GetKeyState(bool isPush)
		{
			return (!isPush) ? this.release_ : this.push_;
		}

		private KeyState push_;

		private KeyState release_;
	}
}
