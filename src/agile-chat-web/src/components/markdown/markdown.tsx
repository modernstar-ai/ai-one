import Markdoc from "@markdoc/markdoc";
import React, { FC } from "react";
import { Citation } from "./citation";
 import { CodeBlock } from "./code-block";
 import { citationConfig } from "./config";
import { MarkdownProvider } from "./markdown-context";
 import { Paragraph } from "./paragraph";

interface Props {
  content: string;
  onCitationClick: (
    previousState: any,
    formData: FormData
  ) => Promise<JSX.Element>;
}

export const Markdown: FC<Props> = (props) => {
  
  let ast;
  try {
    ast = Markdoc.parse(props.content);
  } catch (error) {
    console.error("Error parsing Markdoc content:", error);
    if (error instanceof Error) {
      console.error("Error message:", error.message);
      console.error("Error stack:", error.stack);
    }
    return <div>Error parsing content. Please check the console for more details.</div>;
  }

  let content;
  try {
    content = Markdoc.transform(ast, {
      ...citationConfig,
    });
  } catch (error) {
    console.error("Error transforming AST:", error);
    return <div>Error transforming content.</div>;
  }

  const CitationWrapper: FC<any> = (citationProps) => {
    return (
      <Citation
        items={[
          { name: citationProps.filename, id: citationProps.filename, url: citationProps.url },
        ]}
      />
    );
  };

  const CodeBlockWrapper: FC<any> = (codeBlockProps) => {
    return <CodeBlock {...codeBlockProps} />;
  };

  const ParagraphWrapper: FC<any> = (paragraphProps) => {
    return <Paragraph {...paragraphProps} />;
  };

  const WithContext = () => (
    <MarkdownProvider onCitationClick={props.onCitationClick}>
      {
      Markdoc.renderers.react(content, React, {
        components: { Citation : CitationWrapper, Paragraph: ParagraphWrapper, CodeBlock : CodeBlockWrapper },
      })}
    </MarkdownProvider>
  );
  return <WithContext />;
};