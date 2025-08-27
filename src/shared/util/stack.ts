export class Stack<T extends defined> {
	private items: T[] = [];
	private maxSize?: number;

	constructor(maxSize?: number) {
		this.maxSize = maxSize;
	}

	push(item: T): boolean {
		if (this.maxSize !== undefined && this.items.size() >= this.maxSize) {
			return false;
		}
		this.items.push(item);
		return true;
	}

	pop(): T | undefined {
		return this.items.pop();
	}

	peek(): T | undefined {
		const size = this.items.size();
		return size > 0 ? this.items[size - 1] : undefined;
	}

	isEmpty(): boolean {
		return this.items.size() === 0;
	}

	size(): number {
		return this.items.size();
	}

	clear(): void {
		this.items.clear();
	}

	toArray(): readonly T[] {
		return [...this.items];
	}

	forEach(callback: (item: T, index: number) => void): void {
		this.items.forEach(callback);
	}

	find(predicate: (item: T, index: number) => boolean): T | undefined {
		return this.items.find(predicate);
	}

	filter(predicate: (item: T, index: number) => boolean): T[] {
		return this.items.filter(predicate);
	}

	map<U extends defined>(callback: (item: T, index: number) => U): U[] {
		return this.items.map(callback);
	}

	some(predicate: (item: T, index: number) => boolean): boolean {
		return this.items.some(predicate);
	}

	every(predicate: (item: T, index: number) => boolean): boolean {
		return this.items.every(predicate);
	}

	includes(item: T): boolean {
		return this.items.includes(item);
	}

	indexOf(item: T): number {
		return this.items.indexOf(item);
	}

	peekAt(index: number): T | undefined {
		return this.items[index];
	}

	isFull(): boolean {
		return this.maxSize !== undefined && this.items.size() >= this.maxSize;
	}

	getMaxSize(): number | undefined {
		return this.maxSize;
	}

	setMaxSize(maxSize?: number): void {
		this.maxSize = maxSize;
		if (maxSize !== undefined && this.items.size() > maxSize) {
			// Manually slice the array
			const newItems: T[] = [];
			for (let i = 0; i < maxSize; i++) {
				const item = this.items[i];
				if (item !== undefined) {
					newItems.push(item);
				}
			}
			this.items = newItems;
		}
	}

	clone(): Stack<T> {
		const cloned = new Stack<T>(this.maxSize);
		cloned.items = [...this.items];
		return cloned;
	}

	reverse(): void {
		const reversed: T[] = [];
		for (let i = this.items.size() - 1; i >= 0; i--) {
			const item = this.items[i];
			if (item !== undefined) {
				reversed.push(item);
			}
		}
		this.items = reversed;
	}

	pushAll(items: readonly T[]): number {
		let pushed = 0;
		for (const item of items) {
			if (this.push(item)) {
				pushed++;
			} else {
				break;
			}
		}
		return pushed;
	}

	popMany(count: number): T[] {
		const result: T[] = [];
		for (let i = 0; i < count; i++) {
			const item = this.pop();
			if (item !== undefined) {
				result.push(item);
			} else {
				break;
			}
		}
		return result;
	}

	peekMany(count: number): T[] {
		const size = this.items.size();
		const start = math.max(0, size - count);
		const result: T[] = [];
		for (let i = start; i < size; i++) {
			const item = this.items[i];
			if (item !== undefined) {
				result.push(item);
			}
		}
		return result;
	}

	transfer(target: Stack<T>, count?: number): number {
		const itemsToTransfer = count ?? this.size();
		let transferred = 0;
		
		for (let i = 0; i < itemsToTransfer; i++) {
			const item = this.pop();
			if (item !== undefined && target.push(item)) {
				transferred++;
			} else {
				if (item !== undefined) {
					this.push(item);
				}
				break;
			}
		}
		
		return transferred;
	}
}

export function createStack<T extends defined>(initialItems?: readonly T[], maxSize?: number): Stack<T> {
	const stack = new Stack<T>(maxSize);
	if (initialItems) {
		stack.pushAll(initialItems);
	}
	return stack;
}

export function isStack<T extends defined>(value: unknown): value is Stack<T> {
	return value instanceof Stack;
}