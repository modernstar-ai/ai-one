"use client";
import { FC } from "react";
import { CitationSlider } from "./citation-slider";

interface Citation {
  name: string;  // Can be the citation name
  id: string;
  url: string;
  filename?: string; // Optional field for filename in Markdown
}

interface Props {
  items: Citation[];
}

export const citation = {
  render: "Citation",
  selfClosing: true,
  attributes: {
    items: {
      type: Array,
    },
  },
};

export const Citation: FC<Props> = (props: Props) => {
  // Group citations by name or filename
  const citations = props.items.reduce((acc, citation) => {
    const { name, filename } = citation;
    const groupName = filename || name; // Use filename as fallback if name is not available
    if (!acc[groupName]) {
      acc[groupName] = [];
    }
    acc[groupName].push(citation);
    return acc;
  }, {} as Record<string, Citation[]>);

  return (
    <div className="interactive-citation p-4 border mt-4 flex flex-col rounded-md gap-2">
      {Object.entries(citations).map(([name, items], index: number) => {
        console.log(`Group ${index}: ${name} with items:`, items);

        return (
          <div key={index} className="flex flex-col gap-2">
            <div className="font-semibold text-sm">{name}</div>
            <div className="flex gap-2">
              {items.map((item, index: number) => {
                console.log(`Rendering CitationSlider for item ${index + 1}:`, item);

                return (
                  <div key={index} className="flex flex-col">
                    {/* CitationSlider for citation metadata */}
                    <CitationSlider
                      index={index + 1}
                      name={item.name}
                      id={item.id}
                    />

                    {/* Display filename and clickable URL */}
                    <div className="text-sm text-blue-600">
                      <a 
                        href={item.url} 
                        target="_blank" 
                        rel="noopener noreferrer" 
                        className="hover:underline"
                      >
                        {item.url}
                      </a>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        );
      })}
    </div>
  );
};