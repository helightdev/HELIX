import { defineConfig, defineDocs } from 'fumadocs-mdx/config';
import { metaSchema, pageSchema } from 'fumadocs-core/source/schema';
import type { LanguageInput } from 'shiki';
import mixinExpressionGrammar from './grammars/mixin-expression.tmLanguage.json';

const mixinExpressionLanguage = {
  ...mixinExpressionGrammar,
  name: 'mixin-expression',
  aliases: ['mixin', 'helix-mixin'],
} as LanguageInput;

// You can customize Zod schemas for frontmatter and `meta.json` here
// see https://fumadocs.dev/docs/mdx/collections
export const docs = defineDocs({
  dir: 'content/docs',
  docs: {
    schema: pageSchema,
    postprocess: {
      includeProcessedMarkdown: true,
    },
  },
  meta: {
    schema: metaSchema,
  },
});

export default defineConfig({
  mdxOptions: {
    rehypeCodeOptions: {
      themes: {
        light: 'github-light',
        dark: 'github-dark',
      },
      defaultColor: false,
      langs: [mixinExpressionLanguage],
    },
  },
});
