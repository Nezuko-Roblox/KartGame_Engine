import { getBindingValue, useEventListener, useUnmountEffect } from "@rbxts/pretty-react-hooks";
import type { Binding } from "@rbxts/react";
import React, { useMemo, useState } from "@rbxts/react";
import { createPortal } from "@rbxts/react-roblox";
import { RunService } from "@rbxts/services";

import { palette } from "shared/constants/palette";

import { Frame, type FrameProps } from "./primitive";

interface TransitionProps extends FrameProps {
	Change?: React.InstanceChangeEvent<CanvasGroup | Frame>;
	ClipsDescendants?: Binding<boolean> | boolean;
	DirectChildren?: React.ReactNode;
	Event?: React.InstanceEvent<CanvasGroup | Frame>;
	GroupColor?: Binding<Color3> | Color3;
	GroupTransparency?: Binding<number> | number;
	/** The default properties of the component. */
}

const EPSILON = 0.03;

export function Transition(props: TransitionProps): React.ReactNode {
	const {
		Change: change,
		ClipsDescendants,
		DirectChildren: directChildren,
		Event: event,
		GroupColor: groupColor = palette.white,
		GroupTransparency: groupTransparency = 0,
		Native,
		children,
	} = props;

	const [frame, setFrame] = useState<Frame>();
	const [canvas, setCanvas] = useState<CanvasGroup>();

	const container = useMemo(() => {
		const newContainer = new Instance("Frame");
		newContainer.Size = new UDim2(1, 0, 1, 0);
		newContainer.BackgroundTransparency = 1;
		return newContainer;
	}, []);

	useEventListener(RunService.Heartbeat, () => {
		const transparency = getBindingValue(groupTransparency);
		const color = getBindingValue(groupColor);

		pcall(() => {
			container.Parent = transparency > EPSILON || color !== palette.white ? canvas : frame;
		});
	});

	useUnmountEffect(() => {
		container.Destroy();
	});

	return (
		<Frame
			Native={{
				AnchorPoint: new Vector2(0.5, 0.5),
				BackgroundTransparency: 1,
				Position: new UDim2(0.5, 0, 0.5, 0),
				Size: new UDim2(1, 0, 1, 0),
				...Native,
			}}
		>
			{createPortal(<>{children}</>, container)}

			<canvasgroup
				ref={setCanvas}
				BackgroundTransparency={1}
				Change={change}
				Event={event}
				GroupColor3={groupColor}
				GroupTransparency={groupTransparency}
				Size={new UDim2(1, 0, 1, 0)}
			>
				{directChildren}
			</canvasgroup>

			<frame
				ref={setFrame}
				BackgroundTransparency={1}
				Change={change}
				ClipsDescendants={ClipsDescendants}
				Event={event}
				Size={new UDim2(1, 0, 1, 0)}
			>
				{directChildren}
			</frame>
		</Frame>
	);
}
