import defaultMdxComponents from 'fumadocs-ui/mdx';
import type { MDXComponents } from 'mdx/types';
import {
  FREntityCardLink as FurefEntityCardLink,
  FREntityCodeLink as FurefEntityCodeLink,
  FREntitySymbolLink as FurefEntitySymbolLink,
  type FREntityLinkProps,
} from 'furef';
import { referenceRoute } from '@/lib/shared';

function SymCard(props: FREntityLinkProps) {
  return <FurefEntityCardLink {...props} path={props.path ?? referenceRoute} />;
}

function SymText(props: FREntityLinkProps) {
  return <FurefEntityCodeLink {...props} path={props.path ?? referenceRoute} />;
}

function Sym(props: FREntityLinkProps) {
  return <FurefEntitySymbolLink {...props} path={props.path ?? referenceRoute} />;
}

export function getMDXComponents(components?: MDXComponents) {
  return {
    ...defaultMdxComponents,
      FREntityCardLink: SymCard,
      FREntityCodeLink: SymText,
      FREntitySymbolLink: Sym,
      Sym, SymCard, SymText,
    ...components,
  } satisfies MDXComponents;
}

export const useMDXComponents = getMDXComponents;

declare global {
  type MDXProvidedComponents = ReturnType<typeof getMDXComponents>;
}
