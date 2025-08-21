import React, { forwardRef, useMemo } from "@rbxts/react";

import imageConfig from "assets/configs/images.json";
import type { BindingValue } from "types/util/react";

import type { FrameProps } from "./primitive";

export type SpriteUrl = {
	[K in keyof typeof imageConfig]: `images/${K & string}/${keyof (typeof imageConfig)[K] & string}`;
}[keyof typeof imageConfig];

export interface SpriteProps extends FrameProps<ImageLabel> {
	/** The image to display. */
	Url: BindingValue<SpriteUrl>;
}

/**
 * A component for displaying sprite images from the configured image assets.
 *
 * This component automatically handles sprite sheet coordinates and asset IDs
 * based on the URL format "images/category/spriteName.png".
 *
 * @example
 *
 * ```tsx
 * <Sprite
 * 	Url="images/icons/枇杷.png"
 * 	Native={{
 * 		Size: new UDim2(0, 64, 0, 64),
 * 	}}
 * />;
 * ```
 *
 * @component
 *
 * @see https://developer.roblox.com/en-us/api-reference/class/ImageLabel
 */
export const Sprite = forwardRef((props: Readonly<SpriteProps>, ref: React.Ref<ImageLabel>) => {
	const { CornerRadius, Native, Url, children } = props;

	const spriteConfig = useMemo(() => {
		// 获取 URL 字符串值
		const urlValue = typeIs(Url, "string") ? Url : Url.getValue();

		// 先去掉 .png 后缀
		const urlWithoutExtension = string.gsub(urlValue, "%.png$", "")[0];

		const parts = string.split(urlWithoutExtension, "/");
		if (parts.size() !== 3) {
			warn(
				`Invalid sprite URL format: "${urlValue}". Expected format: "images/category/spriteName.png"`,
			);
			return;
		}

		const [_, category, spriteName] = parts as [string, string, string];

		// 从配置中查找对应的精灵配置
		const categoryConfig = (imageConfig as Record<string, Record<string, unknown>>)[category];
		if (!categoryConfig) {
			warn(`Category "${category}" not found in image config`);
			return;
		}

		const sprite = categoryConfig[spriteName] as
			| undefined
			| {
					Image: string;
					ImageRectOffset: string;
					ImageRectSize: string;
			  };

		if (!sprite) {
			warn(`Sprite "${spriteName}" not found in category "${category}"`);
			return;
		}

		// 解析 ImageRectOffset "[x, y]" 格式
		const offsetMatch = sprite.ImageRectOffset.match("%[(%d+),%s*(%d+)%]");
		const offsetX = tonumber(offsetMatch[0]) ?? 0;
		const offsetY = tonumber(offsetMatch[1]) ?? 0;

		// 解析 ImageRectSize "[width, height]" 格式
		const sizeMatch = sprite.ImageRectSize.match("%[(%d+),%s*(%d+)%]");
		const sizeWidth = tonumber(sizeMatch[0]) ?? 0;
		const sizeHeight = tonumber(sizeMatch[1]) ?? 0;

		return {
			Image: sprite.Image,
			ImageRectOffset: new Vector2(offsetX, offsetY),
			ImageRectSize: new Vector2(sizeWidth, sizeHeight),
		};
	}, [Url]);

	return (
		<imagelabel
			ref={ref}
			BackgroundTransparency={1}
			Image={spriteConfig?.Image ?? ""}
			ImageRectOffset={spriteConfig?.ImageRectOffset ?? new Vector2(0, 0)}
			ImageRectSize={spriteConfig?.ImageRectSize ?? new Vector2(0, 0)}
			Size={new UDim2(1, 0, 1, 0)}
			{...Native}
		>
			{CornerRadius ? <uicorner CornerRadius={CornerRadius} /> : undefined}
			{children}
		</imagelabel>
	);
});
