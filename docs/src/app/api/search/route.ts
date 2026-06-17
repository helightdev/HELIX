import {combinedSource, searchSource} from '@/lib/source';
import { createFromSource } from 'fumadocs-core/search/server';

export const revalidate = false;

export const { staticGET: GET } = createFromSource(searchSource, {
    // https://docs.orama.com/docs/orama-js/supported-languages
    language: 'english',
    async buildIndex(page) {
        const rawStructuredData = page.data.structuredData as
            | Awaited<typeof page.data.structuredData>
            | (() => Awaited<typeof page.data.structuredData>);
        const structuredData =
            typeof rawStructuredData === 'function'
                ? await rawStructuredData()
                : rawStructuredData;

        if (!structuredData) {
            throw new Error(`Cannot find structured data for ${page.url}`);
        }

        return {
            title: page.data.title ?? page.path,
            description: page.data.description,
            url: page.url,
            id: `${page.url}:${page.path}`,
            structuredData,
        };
    },
});
