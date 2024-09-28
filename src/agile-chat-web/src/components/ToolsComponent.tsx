// src/components/ToolsComponent.tsx
import React, { useEffect, useState } from 'react';
import { fetchTools } from '../services/toolservice';
import { Tool } from '../types/Tool';

const ToolsComponent: React.FC = () => {
  const [tools, setTools] = useState<Tool[]>([]);

  useEffect(() => {
    async function getTools() {
      const toolsData = await fetchTools();
      if (toolsData) {
        setTools(toolsData);
      }
    }
    getTools();
  }, []);

  return (
    <div>
      <h1>Tools</h1>
      {tools.length > 0 ? (
        <ul>
          {tools.map((tool) => (
            <li key={tool.id}>{tool.name}</li> // Adjust based on the fields in Tool
          ))}
        </ul>
      ) : (
        <p>No tools available.</p>
      )}
    </div>
  );
};

export default ToolsComponent;
