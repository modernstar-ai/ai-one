import Markdoc from "@markdoc/markdoc";
import React, { FC } from "react";
// import { Citation } from "./citation";
 import { CodeBlock } from "./code-block";
 import { citationConfig } from "./config";
// import { MarkdownProvider } from "./markdown-context";
 import { Paragraph } from "./paragraph";

interface Props {
  content: string;
}

export const Markdown: FC<Props> = (props) => {
  const ast = Markdoc.parse(props.content);

  const content = Markdoc.transform(ast, {
    ...citationConfig,
  });

  const WithContext = () => (
    <>
      {Markdoc.renderers.react(content, React, {
        components: {  Paragraph, CodeBlock },
      })}
    </>
  );

  return <WithContext />;
};