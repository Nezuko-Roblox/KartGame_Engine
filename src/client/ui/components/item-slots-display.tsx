import React, { useEffect } from "@rbxts/react";
import { useMotion } from "client/ui/hooks";
import { springs } from "client/constants";

interface ItemSlotProps {
	item?: string;
	index: number;
	isActive: boolean;
}

function ItemSlot({ item, index, isActive }: ItemSlotProps) {
	const [scale, scaleMotion] = useMotion(1);
	const [transparency, transparencyMotion] = useMotion(0);

	useEffect(() => {
		if (isActive) {
			scaleMotion.spring(1.05, springs.responsive);
		} else {
			scaleMotion.spring(1, springs.responsive);
		}
	}, [isActive]);

	useEffect(() => {
		if (item) {
			transparencyMotion.spring(0, springs.responsive);
		} else {
			transparencyMotion.spring(0.5, springs.responsive);
		}
	}, [item]);

	return (
		<frame
			key={`slot-${index}`}
			Size={new UDim2(0, 50, 0, 50)}
			Position={new UDim2(0, index * 55, 0, 0)}
			BackgroundColor3={isActive ? new Color3(0.3, 0.7, 1) : new Color3(0.2, 0.2, 0.2)}
			BackgroundTransparency={transparency}
			BorderSizePixel={isActive ? 2 : 1}
			BorderColor3={isActive ? new Color3(0.5, 0.8, 1) : new Color3(0.4, 0.4, 0.4)}
		>
			<uiscale Scale={scale} />
			<uicorner CornerRadius={new UDim(0, 6)} />
			
			<textlabel
				Size={UDim2.fromScale(0.3, 0.3)}
				Position={UDim2.fromScale(0.02, 0.02)}
				BackgroundTransparency={1}
				Text={`${index + 1}`}
				TextColor3={new Color3(0.6, 0.6, 0.6)}
				TextScaled={true}
				Font={Enum.Font.SourceSans}
			/>

			<textlabel
				Size={UDim2.fromScale(0.9, 0.5)}
				Position={UDim2.fromScale(0.05, 0.35)}
				BackgroundTransparency={1}
				Text={item ?? "空"}
				TextColor3={item ? new Color3(1, 1, 1) : new Color3(0.5, 0.5, 0.5)}
				TextScaled={true}
				Font={item ? Enum.Font.SourceSansBold : Enum.Font.SourceSans}
			/>

			{isActive && (
				<frame
					Size={UDim2.fromScale(1, 1)}
					BackgroundTransparency={1}
					BorderSizePixel={2}
					BorderColor3={new Color3(1, 1, 0)}
				>
					<uicorner CornerRadius={new UDim(0, 6)} />
				</frame>
			)}
		</frame>
	);
}

interface ItemSlotsDisplayProps {
	items: (string | undefined)[];
	currentSlot?: number;
	maxSlots: number;
}

export function ItemSlotsDisplay({ items, currentSlot = 0, maxSlots }: ItemSlotsDisplayProps) {
	const [containerTransparency, containerTransparencyMotion] = useMotion(0.3);

	return (
		<frame
			Size={new UDim2(0, maxSlots * 55 + 15, 0, 70)}
			Position={new UDim2(0, 20, 0, 20)}
			AnchorPoint={new Vector2(0, 0)}
			BackgroundColor3={new Color3(0, 0, 0)}
			BackgroundTransparency={containerTransparency}
			BorderSizePixel={0}
		>
			<uicorner CornerRadius={new UDim(0, 8)} />
			<uipadding
				PaddingLeft={new UDim(0, 8)}
				PaddingRight={new UDim(0, 8)}
				PaddingTop={new UDim(0, 8)}
				PaddingBottom={new UDim(0, 8)}
			/>

			<frame
				Size={UDim2.fromScale(1, 1)}
				BackgroundTransparency={1}
			>
				{(() => {
					const slots = [];
					for (let i = 0; i < maxSlots; i++) {
						slots.push(
							<ItemSlot
								key={i}
								item={items[i]}
								index={i}
								isActive={i === currentSlot}
							/>
						);
					}
					return slots;
				})()}
			</frame>

			<textlabel
				Size={new UDim2(1, 0, 0, 12)}
				Position={new UDim2(0, 0, 1, 2)}
				BackgroundTransparency={1}
				Text="SPACE使用"
				TextColor3={new Color3(0.6, 0.6, 0.6)}
				TextScaled={false}
				TextSize={10}
				Font={Enum.Font.SourceSans}
				TextXAlignment={Enum.TextXAlignment.Center}
			/>
		</frame>
	);
}