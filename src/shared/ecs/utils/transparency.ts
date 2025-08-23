/**
 * 设置模型及其子部件的透明度.
 *
 * @param model - 要设置透明度的模型.
 * @param transparency - 透明度值.
 */
export function SetTransparency(model: Model, transparency: number): void {
	const part = model.PrimaryPart;
	if (!part) {
		return;
	}

	if (part.Name !== "HumanoidRootPart") {
		if (part.GetAttribute("_originalTransparency") === undefined) {
			part.SetAttribute("_originalTransparency", part.Transparency);
		}

		part.Transparency = transparency;
	}

	for (const child of model.GetDescendants()) {
		if ((child.IsA("BasePart") || child.IsA("Decal")) && child.Name !== "HumanoidRootPart") {
			if (child.GetAttribute("_originalTransparency") === undefined) {
				child.SetAttribute("_originalTransparency", (child as BasePart).Transparency);
			}

			(child as BasePart).Transparency = transparency;
		}
	}
}

/**
 * 重置模型及其子部件的透明度到原始值.
 *
 * @param model - 要重置透明度的模型.
 */
export function ResetTransparency(model: Model): void {
	const part = model.PrimaryPart;
	if (!part) {
		return;
	}

	if (
		part.Name !== "HumanoidRootPart" &&
		part.GetAttribute("_originalTransparency") !== undefined
	) {
		part.Transparency = part.GetAttribute("_originalTransparency") as number;
	}

	for (const child of model.GetDescendants()) {
		if (
			(child.IsA("BasePart") || child.IsA("Decal")) &&
			child.Name !== "HumanoidRootPart" &&
			child.GetAttribute("_originalTransparency") !== undefined
		) {
			(child as BasePart).Transparency = child.GetAttribute(
				"_originalTransparency",
			) as number;
		}
	}
}
