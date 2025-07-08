// SideNavButton.tsx
import React from 'react';
import { Link } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Tooltip, TooltipTrigger, TooltipContent } from '@/components/ui/tooltip';

interface SideNavButtonProps {
  path: string;
  label: string;
  Icon: React.ComponentType<React.SVGProps<SVGSVGElement>>;
  accessKey: string;
}

const SideNavButton: React.FC<SideNavButtonProps> = ({ path, label, Icon, accessKey }) => {
  return (
    <Tooltip key={path}>
      <TooltipTrigger asChild >
        <Link to={path} aria-label={label} accessKey={accessKey} >
          <Button variant="ghost" size="icon" tabIndex={-1} aria-label={label} >
            <Icon className="h-5 w-5" />
          </Button>
        </Link>
      </TooltipTrigger>
      <TooltipContent side="right">{label}</TooltipContent>
    </Tooltip>
  );
};

export default SideNavButton;