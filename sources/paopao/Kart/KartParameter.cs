using System;

public class KartParameter
{
	public KartParameter(byte body, byte character, string name, PlayerType type, string param)
	{
		this.body_ = body;
		this.character_ = character;
		this.name_ = name;
		this.type_ = type;
		this.reserved_ = param;
	}

	public KartParameter(byte body, byte character, string name, PlayerType type, string param, string id)
		: this(body, character, name, type, param)
	{
		this.id_ = id;
	}

	public override string ToString()
	{
		return string.Format("<KartParameter> body_={0}, character_={1}, name_={2}, type_={3}, reserved_ = {4}, id_={5}", new object[] { this.body_, this.character_, this.name_, this.type_, this.reserved_, this.id_ });
	}

	public byte body_;

	public byte character_;

	public string name_;

	public PlayerType type_;

	public string reserved_;

	public string id_;
}
