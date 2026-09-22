import React from "react";
interface PageHeaderProps {
  title: string;
  description?: string;
  badge?: React.ReactNode;
  children?: React.ReactNode;
  //  For action buttons (Export, Create, etc.)
}

export const PageHeader: React.FC<PageHeaderProps> = ({
  title,
  description,
  badge,
  children,
}) => {
  return (
    <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-border/60 pb-5 mb-6">
      <div className="space-y-1">
        <div className="flex items-center gap-2.5">
          <h1 className="text-2xl font-bold tracking-tight text-foreground">
            {title}
          </h1>
          {badge}
        </div>
        {description && (
          <p className="text-sm text-muted-foreground">{description}</p>
        )}
      </div>
      {children && (
        <div className="flex items-center gap-2.5 shrink-0">{children}</div>
      )}
    </div>
  );
};
