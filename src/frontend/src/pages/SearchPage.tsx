// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
import React, { useState, useEffect, useCallback } from 'react'
import { useSearchParams } from 'react-router-dom'
import searchService from '../services/searchService'
import type { SearchResult } from '../types'

interface SearchDocument extends Record<string, unknown> {
  _score?: number
}

const SearchPage: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams()
  const [query, setQuery] = useState(searchParams.get('q') || '')
  const [index, setIndex] = useState(searchParams.get('index') || 'core-index')
  const [results, setResults] = useState<SearchResult<SearchDocument> | null>(
    null
  )
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [page, setPage] = useState(1)
  const pageSize = 20

  const search = useCallback(
    async (searchQuery: string, searchIndex: string, pageNum: number = 1) => {
      if (!searchQuery.trim()) return

      setLoading(true)
      setError(null)

      try {
        const result = await searchService.search({
          query: searchQuery,
          index: searchIndex,
          page: pageNum,
          pageSize,
        })

        setResults(result)
        setPage(pageNum)

        // Update URL parameters
        const newSearchParams = new URLSearchParams()
        newSearchParams.set('q', searchQuery)
        newSearchParams.set('index', searchIndex)
        if (pageNum > 1) newSearchParams.set('page', pageNum.toString())
        setSearchParams(newSearchParams)
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Search failed')
      } finally {
        setLoading(false)
      }
    },
    [setSearchParams]
  )

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault()
    search(query, index, 1)
  }

  const handlePageChange = (newPage: number) => {
    search(query, index, newPage)
  }

  const handleIndexChange = (newIndex: string) => {
    setIndex(newIndex)
    if (query.trim()) {
      search(query, newIndex, 1)
    }
  }

  useEffect(() => {
    const urlQuery = searchParams.get('q')
    const urlIndex = searchParams.get('index')
    const urlPage = parseInt(searchParams.get('page') || '1')

    if (urlQuery) {
      setQuery(urlQuery)
      setIndex(urlIndex || 'core-index')
      search(urlQuery, urlIndex || 'core-index', urlPage)
    }
  }, [search, searchParams])

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="bg-white rounded-lg shadow-sm p-6">
          <h1 className="text-2xl font-bold text-gray-900 mb-6">Search</h1>

          {/* Search Form */}
          <form onSubmit={handleSearch} className="mb-6">
            <div className="flex gap-4">
              <div className="flex-1">
                <input
                  type="text"
                  value={query}
                  onChange={e => setQuery(e.target.value)}
                  placeholder="Enter search query..."
                  className="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
              <div className="w-48">
                <select
                  value={index}
                  onChange={e => handleIndexChange(e.target.value)}
                  aria-label="Search index"
                  className="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option value="core-index">All Content</option>
                  <option value="users">Users</option>
                  <option value="payments">Payments</option>
                  <option value="subscriptions">Subscriptions</option>
                </select>
              </div>
              <button
                type="submit"
                disabled={loading}
                className="px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50"
              >
                {loading ? 'Searching...' : 'Search'}
              </button>
            </div>
          </form>

          {/* Error Message */}
          {error && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-md">
              <p className="text-red-800">{error}</p>
            </div>
          )}

          {/* Search Results */}
          {results && (
            <div>
              <div className="mb-4 text-sm text-gray-600">
                Found {results.totalHits} results in {results.took}ms
                {results.page > 1 &&
                  ` (Page ${results.page} of ${results.totalPages})`}
              </div>

              {/* Results List */}
              <div className="space-y-4">
                {results.documents.map((document, index) => (
                  <div
                    key={index}
                    className="border border-gray-200 rounded-lg p-4 hover:bg-gray-50"
                  >
                    <div className="text-sm text-gray-500 mb-2">
                      Score: {document._score?.toFixed(2) || 'N/A'}
                    </div>
                    <pre className="text-sm text-gray-800 whitespace-pre-wrap">
                      {JSON.stringify(document, null, 2)}
                    </pre>
                  </div>
                ))}
              </div>

              {/* Pagination */}
              {results.totalPages > 1 && (
                <div className="mt-6 flex justify-center">
                  <nav className="flex space-x-2">
                    <button
                      onClick={() => handlePageChange(page - 1)}
                      disabled={page <= 1}
                      className="px-3 py-2 text-sm font-medium text-gray-500 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      Previous
                    </button>

                    {Array.from(
                      { length: Math.min(5, results.totalPages) },
                      (_, i) => {
                        const pageNum =
                          Math.max(
                            1,
                            Math.min(results.totalPages - 4, page - 2)
                          ) + i
                        return (
                          <button
                            key={pageNum}
                            onClick={() => handlePageChange(pageNum)}
                            className={`px-3 py-2 text-sm font-medium rounded-md ${
                              pageNum === page
                                ? 'bg-blue-600 text-white'
                                : 'text-gray-500 bg-white border border-gray-300 hover:bg-gray-50'
                            }`}
                          >
                            {pageNum}
                          </button>
                        )
                      }
                    )}

                    <button
                      onClick={() => handlePageChange(page + 1)}
                      disabled={page >= results.totalPages}
                      className="px-3 py-2 text-sm font-medium text-gray-500 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      Next
                    </button>
                  </nav>
                </div>
              )}
            </div>
          )}

          {/* No Results */}
          {results && results.documents.length === 0 && !loading && (
            <div className="text-center py-8">
              <p className="text-gray-500">No results found for "{query}"</p>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}

export default SearchPage
