import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

//"cn"  is a common abbreviation for "class names."
//It combines and merges class names using the clsx and twMerge libraries. Here's what it does:
//clsx: Constructs className strings conditionally.
//twMerge: Merges Tailwind CSS class names to avoid conflicts.
//The cn function takes multiple class name inputs, processes them with clsx to handle conditional logic,
//and then merges the resulting class names using twMerge.
//This ensures that the final class name string is optimized and free of conflicts.

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}
