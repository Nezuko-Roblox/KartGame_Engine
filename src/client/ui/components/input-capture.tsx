import { useEventListener } from "@rbxts/pretty-react-hooks";
import React, { useState } from "@rbxts/react";
import { UserInputService } from "@rbxts/services";

import { IS_EDIT } from "shared/constants";

import { Group } from "./primitive";

interface InputCaptureProps {
	readonly anchorPoint?: Vector2;
	readonly onInputBegan?: (rbx: Frame, input: InputObject) => void;
	readonly onInputChanged?: (rbx: Frame, input: InputObject) => void;
	readonly onInputEnded?: (rbx: Frame, input: InputObject) => void;
	readonly position?: UDim2;
	readonly size?: UDim2;
}

export function InputCapture({
	anchorPoint,
	onInputBegan,
	onInputChanged,
	onInputEnded,
	position,
	size,
}: InputCaptureProps): React.ReactNode {
	const [frame, setFrame] = useState<Frame>();

	useEventListener(UserInputService.InputBegan, (input, gameProcessed) => {
		if (frame && !IS_EDIT && !gameProcessed) {
			onInputBegan?.(frame, input);
		}
	});

	useEventListener(UserInputService.InputEnded, input => {
		if (frame && !IS_EDIT) {
			onInputEnded?.(frame, input);
		}
	});

	useEventListener(UserInputService.InputChanged, input => {
		if (frame && !IS_EDIT) {
			onInputChanged?.(frame, input);
		}
	});

	return (
		<Group
			ref={setFrame}
			Event={{
				InputBegan: IS_EDIT ? onInputBegan : undefined,
				InputChanged: IS_EDIT ? onInputChanged : undefined,
				InputEnded: IS_EDIT ? onInputEnded : undefined,
			}}
			Native={{
				AnchorPoint: anchorPoint,
				Position: position,
				Size: size,
			}}
		/>
	);
}
