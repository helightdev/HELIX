import { docs } from 'collections/server';
import { loader } from 'fumadocs-core/source';
import { lucideIconsPlugin } from 'fumadocs-core/source/plugins/lucide-icons';
import { createDocfxSource, isFRPageData, pathSegments } from 'furef';
import { docsContentRoute, docsImageRoute, docsRoute, referenceRoute } from './shared';

const docfx = createDocfxSource({
    dir: 'docfx',
    mode: 'pages',
    title: 'C# Reference',
    baseUrl: '',
    path: referenceRoute,
    navigation: {
        root: 'none',
        namespaces: 'folder',
    },
    externalLinkPrefixes: {
        "UnityEngine.InputSystem": (qualifiedId, prefix) =>
            `https://docs.unity3d.com/Packages/com.unity.inputsystem@1.19//api/${qualifiedId}.html`,
        "UnityEngine": (qualifiedId, prefix) =>
            `https://docs.unity3d.com/ScriptReference/${qualifiedId
                .replace("UnityEngine.", "")
                .replace("UnityEditor.", "")
            }.html`,
    },
    sourceBasePath: 'src/HELIX/Assets/Plugins/',
    vcsRoot: 'https://github.com/helightdev/HELIX/tree/main/src/HELIX/Assets/Plugins/',
    collapseAllMembers: false,
    expandable: false,
    hierarchicalNamespaces: true,
    showSummaryInList: true,
    compactTreeNames: true
});

export const docfxSource = loader({
    baseUrl: '',
    source: docfx.source
});

export const referenceTree = docfx.pageTree;

const referenceRouteSlugs = pathSegments(referenceRoute);

export function getReferencePage(slug: string[] = []) {
    return docfxSource.getPage([...referenceRouteSlugs, ...slug]);
}

export function getReferenceRouteSlugs(page: (typeof docfxSource)['$inferPage']) {
    const slugs = page.slugs;
    const hasReferencePrefix = referenceRouteSlugs.every((part, index) => slugs[index] === part);

    return hasReferencePrefix ? slugs.slice(referenceRouteSlugs.length) : slugs;
}


// See https://fumadocs.dev/docs/headless/source-api for more info
export const source = loader({
  baseUrl: docsRoute,
  source: docs.toFumadocsSource(),
  plugins: [lucideIconsPlugin()],
});

export const combinedSource = {
    ...source,
    getPages: () => [...source.getPages(), ...docfxSource.getPages()],
    getPageTree: (locale?: string) => {
        const docsTree = source.getPageTree(locale);

        return {
            ...docsTree,
            children: [...docsTree.children, ...referenceTree.children],
        };
    },
} as typeof source;

export const searchSource = {
    ...source,
    getPages: () => [...source.getPages(), ...docfxSource.getPages()],
    getPageTree: (locale?: string) => {
        const docsTree = source.getPageTree(locale);

        return {
            ...docsTree,
            children: [...docsTree.children, ...referenceTree.children],
        };
    },
} as typeof source;


export function getPageImage(page: (typeof source)['$inferPage']) {
  const segments = [...page.slugs, 'image.png'];

  return {
    segments,
    url: `${docsImageRoute}/${segments.join('/')}`,
  };
}

export function getPageMarkdownUrl(page: (typeof source)['$inferPage']) {
  const segments = [...page.slugs, 'content.md'];

  return {
    segments,
    url: `${docsContentRoute}/${segments.join('/')}`,
  };
}

export async function getLLMText(page: (typeof source)['$inferPage'] | (typeof docfxSource)['$inferPage']) {
  const processed = isFRPageData(page.data)
      ? page.data.docfx.markdown
      : await page.data.getText('processed');

  return `# ${page.data.title} (${page.url})

${processed}`;
}
