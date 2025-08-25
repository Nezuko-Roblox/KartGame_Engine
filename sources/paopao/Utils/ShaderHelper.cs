using System;

public static class ShaderHelper
{
	public static string noTextureShader_ = "Shader \"NoTextueShader\" {\r\n\tProperties {\r\n\t}\r\n\tSubShader {\r\n\t\tPass {\r\n            BindChannels {\r\n               Bind \"Vertex\", vertex\r\n               Bind \"Color\", color\r\n            }\r\n            Lighting Off\r\n\t\t}\r\n\t}\r\n\tFallback off\r\n}";
}
